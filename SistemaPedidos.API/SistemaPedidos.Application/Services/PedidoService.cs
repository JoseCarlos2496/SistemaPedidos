using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SistemaPedidos.Application.DTOs;
using SistemaPedidos.Application.Interfaces;
using SistemaPedidos.Domain.Entities;
using SistemaPedidos.Domain.Exceptions;
using SistemaPedidos.Domain.Interfaces;

namespace SistemaPedidos.Application.Services
{
    /// <summary>
    /// Servicio de aplicación para gestión del ciclo de vida de pedidos.
    /// Coordina validaciones, transacciones, servicios externos y auditoría.
    /// </summary>
    /// <remarks>
    /// Patrones implementados:
    /// - Unit of Work (vía IOrkestador)
    /// - Execution Strategy (reintentos automáticos SQL Server)
    /// - Exception Translation (técnicas → dominio)
    /// 
    /// No atrapa excepciones de dominio, las propaga al middleware.
    /// Solo convierte excepciones técnicas (DbUpdateException, SqlException) en excepciones de dominio.
    /// </remarks>
    public class PedidoService : IPedidoService
    {
        private readonly IOrkestador _orkestor;
        private readonly IValidacionExternaService _validacionService;
        private readonly ILogger<PedidoService> _logger;

        /// <summary>
        /// Constructor con inyección de dependencias.
        /// </summary>
        /// <param name="orkestor">Unit of Work para repositorios, transacciones y estrategia de reintentos</param>
        /// <param name="validacionService">Servicio de validación externa de clientes</param>
        /// <param name="logger">Logger para eventos y errores</param>
        public PedidoService(
            IOrkestador orkestor,
            IValidacionExternaService validacionService,
            ILogger<PedidoService> logger)
        {
            _orkestor = orkestor ?? throw new ArgumentNullException(nameof(orkestor));
            _validacionService = validacionService ?? throw new ArgumentNullException(nameof(validacionService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Registra un nuevo pedido con validación completa y transaccionalidad.
        /// </summary>
        /// <remarks>
        /// Flujo: Validación → Transacción → Auditoría inicio → Cálculo total → 
        /// Validación externa → Persistencia → Auditoría fin → Commit.
        /// 
        /// Ejecutado dentro de ExecuteInStrategyAsync para reintentos automáticos en errores transitorios.
        /// Rollback automático en cualquier error con registro de auditoría.
        /// </remarks>
        public async Task<PedidoResponse> RegistrarPedidoAsync(PedidoRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _orkestor.ExecuteInStrategyAsync(async () =>
                {
                    _logger.LogInformation(
                        "=== INICIO REGISTRO PEDIDO === Cliente: {ClienteId}, Usuario: {Usuario}, Items: {Items}",
                        request?.ClienteId,
                        request?.Usuario,
                        request?.Items?.Count ?? 0
                    );

                    ValidarRequest(request);
                    await IniciarTransaccionAsync(cancellationToken);

                    try
                    {
                        await RegistrarAuditoriaAsync(
                            "PEDIDO_INICIADO",
                            $"Iniciando registro para cliente {request.ClienteId} por usuario {request.Usuario}",
                            cancellationToken
                        );

                        decimal total = CalcularTotal(request);
                        await _validacionService.ValidarPedidoAsync(request.ClienteId, total);
                        var pedidoCreado = await CrearYGuardarPedidoAsync(request, total, cancellationToken);

                        await RegistrarAuditoriaAsync(
                            "PEDIDO_CREADO",
                            $"Pedido {pedidoCreado.Id} creado. Cliente: {request.ClienteId}, Total: ${total}, Items: {request.Items.Count}",
                            cancellationToken
                        );

                        await _orkestor.CommitTransactionAsync(cancellationToken);

                        _logger.LogInformation(
                            "=== PEDIDO EXITOSO === PedidoId: {PedidoId}, Total: ${Total:}",
                            pedidoCreado.Id,
                            pedidoCreado.Total
                        );

                        return CrearRespuestaExitosa(pedidoCreado, request.Items.Count);
                    }
                    catch (Exception ex)
                    {
                        await RollbackSafeAsync(cancellationToken);
                        await RegistrarAuditoriaAsync(
                            "PEDIDO_ERROR",
                            $"Error al procesar pedido. Cliente: {request?.ClienteId}, Tipo: {ex.GetType().Name}, Mensaje: {ex.Message}",
                            cancellationToken
                        );
                        throw;
                    }
                });
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("estrategia"))
            {
                _logger.LogCritical(ex, "Error de configuración de estrategia de ejecución");
                throw new ConfigurationException(
                    "Error en la configuración de la estrategia de ejecución de base de datos",
                    "ExecutionStrategy",
                    ex
                );
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error al actualizar base de datos");
                throw new DatabaseException("Error al guardar el pedido en la base de datos", ex);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Error de SQL Server: {Number}", ex.Number);
                throw new DatabaseException(
                    $"Error de SQL Server (código {ex.Number}): {ObtenerMensajeSqlException(ex)}",
                    ex
                );
            }
            catch (TimeoutException ex)
            {
                _logger.LogError(ex, "Timeout en operación de base de datos");
                throw new TransactionException("La operación excedió el tiempo límite", ex);
            }
        }

        #region Validación

        /// <summary>
        /// Valida datos del request contra reglas de formato y negocio básicas.
        /// </summary>
        /// <remarks>
        /// Valida: Request no nulo, ClienteId > 0, Usuario entre 3-100 caracteres,
        /// Items entre 1-100 elementos. Cada item se valida individualmente.
        /// </remarks>
        private void ValidarRequest(PedidoRequest request)
        {
            _logger.LogDebug("Validando request...");

            if (request == null)
                throw new ValidationException("El request no puede ser nulo");

            if (request.ClienteId <= 0)
            {
                var ex = new ValidationException($"ClienteId inválido: {request.ClienteId}");
                ex.AddMetadata("ClienteId", request.ClienteId);
                throw ex;
            }

            if (string.IsNullOrWhiteSpace(request.Usuario))
                throw new ValidationException("El usuario es requerido");

            if (request.Usuario.Length < 3 || request.Usuario.Length > 100)
            {
                var ex = new ValidationException(
                    $"El usuario debe tener entre 3 y 100 caracteres. Actual: {request.Usuario.Length}"
                );
                ex.AddMetadata("Longitud", request.Usuario.Length);
                throw ex;
            }

            if (request.Items == null || !request.Items.Any())
                throw new ValidationException("El pedido debe contener al menos un item");

            if (request.Items.Count > 100)
            {
                var ex = new ValidationException($"Máximo 100 items permitidos. Actual: {request.Items.Count}");
                ex.AddMetadata("CantidadItems", request.Items.Count);
                throw ex;
            }

            for (int i = 0; i < request.Items.Count; i++)
            {
                ValidarItem(request.Items[i], i);
            }

            _logger.LogDebug("Validación completada exitosamente");
        }

        /// <summary>
        /// Valida un item individual del pedido.
        /// </summary>
        /// <remarks>
        /// Valida ProductoId > 0, Cantidad entre 1-10,000, Precio entre $0.01-$999,999.99.
        /// Agrega metadata con ItemIndex y valor inválido para debugging.
        /// </remarks>
        private void ValidarItem(PedidoItemRequest item, int index)
        {
            var pos = index + 1;

            if (item.ProductoId <= 0)
            {
                var ex = new ValidationException($"Item {pos}: ProductoId inválido ({item.ProductoId})");
                ex.AddMetadata("ItemIndex", index);
                ex.AddMetadata("ProductoId", item.ProductoId);
                throw ex;
            }

            if (item.Cantidad <= 0 || item.Cantidad > 10000)
            {
                var ex = new ValidationException(
                    $"Item {pos}: Cantidad debe estar entre 1 y 10,000 ({item.Cantidad})"
                );
                ex.AddMetadata("ItemIndex", index);
                ex.AddMetadata("Cantidad", item.Cantidad);
                throw ex;
            }

            if (item.Precio <= 0 || item.Precio > 999999.99m)
            {
                var ex = new ValidationException(
                    $"Item {pos}: Precio debe estar entre $0.01 y $999,999.99 (${item.Precio})"
                );
                ex.AddMetadata("ItemIndex", index);
                ex.AddMetadata("Precio", item.Precio);
                throw ex;
            }
        }

        /// <summary>
        /// Calcula el total del pedido sumando cantidad * precio de todos los items.
        /// </summary>
        /// <remarks>
        /// Usa aritmética checked para detectar overflow.
        /// Valida total > 0 y <= $999,999,999.99.
        /// Usa decimal para precisión monetaria exacta.
        /// </remarks>
        private decimal CalcularTotal(PedidoRequest request)
        {
            try
            {
                _logger.LogDebug("Calculando total...");

                decimal total = 0;
                foreach (var item in request.Items)
                {
                    checked { total += item.Cantidad * item.Precio; }
                }

                if (total <= 0)
                {
                    var ex = new ValidationException($"Total inválido: ${total}");
                    ex.AddMetadata("Total", total);
                    throw ex;
                }

                _logger.LogInformation("Total calculado: ${Total} ({Items} items)", total, request.Items.Count);
                return total;
            }
            catch (OverflowException ex)
            {
                _logger.LogError(ex, "Overflow al calcular total");
                throw new ValidationException("El total excede el límite por desbordamiento aritmético", ex);
            }
        }

        #endregion

        #region Transacción y Persistencia

        /// <summary>
        /// Inicia una transacción de base de datos para garantizar atomicidad.
        /// </summary>
        /// <remarks>
        /// Todas las operaciones (pedido, detalles, auditoría) se ejecutan como unidad atómica.
        /// Se ejecuta dentro de ExecuteInStrategyAsync para reintentos automáticos.
        /// </remarks>
        private async Task IniciarTransaccionAsync(CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogDebug("Iniciando transacción...");
                await _orkestor.BeginTransactionAsync(cancellationToken);
                _logger.LogDebug("Transacción iniciada exitosamente");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Error: Ya existe una transacción activa");
                throw new TransactionException("No se pudo iniciar la transacción. Ya existe una activa.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al iniciar transacción");
                throw new TransactionException($"Error al iniciar transacción: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Crea entidades PedidoCabecera y PedidoDetalle y las persiste en base de datos.
        /// </summary>
        /// <remarks>
        /// Genera PedidoCabecera con lista de PedidoDetalle.
        /// SaveChangesAsync ejecuta INSERT y genera IDs (IDENTITY).
        /// Valida que el ID generado sea > 0.
        /// </remarks>
        private async Task<PedidoCabecera> CrearYGuardarPedidoAsync(
            PedidoRequest request,
            decimal total,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Creando entidad de pedido...");

                var pedido = new PedidoCabecera
                {
                    ClienteId = request.ClienteId,
                    Fecha = DateTime.Now,
                    Total = total,
                    Usuario = request.Usuario,
                    Detalles = request.Items.Select(item => new PedidoDetalle
                    {
                        ProductoId = item.ProductoId,
                        Cantidad = item.Cantidad,
                        Precio = item.Precio
                    }).ToList()
                };

                var pedidoCreado = await _orkestor.Pedidos.AddAsync(pedido);
                await _orkestor.SaveChangesAsync(cancellationToken);

                if (pedidoCreado.Id <= 0)
                {
                    throw new DatabaseException("El pedido fue guardado pero no se generó un ID válido");
                }

                _logger.LogInformation(
                    "Pedido guardado exitosamente - Id: {PedidoId}, Items: {CantidadItems}",
                    pedidoCreado.Id,
                    pedidoCreado.Detalles?.Count ?? 0
                );

                return pedidoCreado;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error al actualizar base de datos");
                throw new DatabaseException(
                    "Error al guardar el pedido: " + ObtenerDetallesDbUpdateException(ex),
                    ex
                );
            }
            catch (DatabaseException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al crear pedido");
                throw new DatabaseException("Error inesperado al crear y guardar el pedido", ex);
            }
        }

        /// <summary>
        /// Registra evento de auditoría de forma no bloqueante.
        /// </summary>
        /// <remarks>
        /// NO lanza excepciones. Si falla, loguea warning y continúa.
        /// Se guarda dentro de la misma transacción del pedido (Transactional Outbox).
        /// </remarks>
        private async Task RegistrarAuditoriaAsync(
            string evento,
            string descripcion,
            CancellationToken cancellationToken)
        {
            try
            {
                await _orkestor.LogAuditoria.RegistrarEventoAsync(evento, descripcion);
                await _orkestor.SaveChangesAsync(cancellationToken);
                _logger.LogDebug("Auditoría registrada: {Evento}", evento);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error al registrar auditoría - {Evento}. Proceso continúa.", evento);
            }
        }

        #endregion

        #region Auxiliares

        /// <summary>
        /// Ejecuta rollback de transacción sin lanzar excepciones.
        /// </summary>
        /// <remarks>
        /// Si el rollback falla, solo loguea el error sin interferir con la excepción original.
        /// </remarks>
        private async Task RollbackSafeAsync(CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogDebug("Ejecutando rollback de transacción...");
                await _orkestor.RollbackTransactionAsync(cancellationToken);
                _logger.LogDebug("Rollback completado");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al ejecutar rollback");
            }
        }

        /// <summary>
        /// Extrae detalles legibles de DbUpdateException para debugging.
        /// </summary>
        /// <remarks>
        /// Obtiene tipo de entidad, estado y código SQL si está disponible.
        /// Códigos comunes: 2627/2601 (unique), 547 (FK), -1/-2 (timeout).
        /// </remarks>
        private string ObtenerDetallesDbUpdateException(DbUpdateException ex)
        {
            var detalles = new List<string>();

            foreach (var entry in ex.Entries)
            {
                detalles.Add($"{entry.Entity.GetType().Name}:{entry.State}");
            }

            if (ex.InnerException is SqlException sqlEx)
            {
                detalles.Add($"SQL{sqlEx.Number}");

                var mensaje = sqlEx.Number switch
                {
                    2627 or 2601 => "Restricción única violada",
                    547 => "Clave foránea violada",
                    -1 or -2 => "Timeout de conexión",
                    _ => sqlEx.Message
                };

                detalles.Add(mensaje);
            }

            return string.Join(" | ", detalles);
        }

        /// <summary>
        /// Mapea código de error SQL Server a mensaje legible.
        /// </summary>
        private string ObtenerMensajeSqlException(SqlException ex)
        {
            return ex.Number switch
            {
                2627 or 2601 => "Registro duplicado (restricción única)",
                547 => "Referencia inválida (clave foránea)",
                -1 or -2 => "Timeout de conexión",
                1205 => "Deadlock detectado",
                _ => ex.Message
            };
        }

        /// <summary>
        /// Crea DTO de respuesta exitosa desde entidad persistida.
        /// </summary>
        /// <remarks>
        /// Mapea PedidoCabecera → PedidoResponse.
        /// Desacopla entidad de dominio de contrato HTTP.
        /// </remarks>
        private PedidoResponse CrearRespuestaExitosa(PedidoCabecera pedido, int cantidadItems)
        {
            return new PedidoResponse
            {
                PedidoId = pedido.Id,
                ClienteId = pedido.ClienteId,
                Fecha = pedido.Fecha,
                Total = pedido.Total,
                Usuario = pedido.Usuario,
                CantidadItems = cantidadItems
            };
        }

        #endregion
    }
}