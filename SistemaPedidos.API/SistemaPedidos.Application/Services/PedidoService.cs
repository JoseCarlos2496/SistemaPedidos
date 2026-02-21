using Microsoft.Extensions.Logging;
using SistemaPedidos.Application.DTOs;
using SistemaPedidos.Application.Interfaces;
using SistemaPedidos.Domain.Entities;
using SistemaPedidos.Domain.Interfaces;

namespace SistemaPedidos.Application.Services
{
    public class PedidoService : IPedidoService
    {
        private readonly IOrkestador _orkestor;
        private readonly IValidacionExternaService _validacionService;
        private readonly ILogger<PedidoService> _logger;

        public PedidoService(
            IOrkestador orkestor,
            IValidacionExternaService validacionService,
            ILogger<PedidoService> logger)
        {
            _orkestor = orkestor;
            _validacionService = validacionService;
            _logger = logger;
        }

        public async Task<PedidoResponse> RegistrarPedidoAsync(PedidoRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation(
                    "Iniciando registro de pedido - Cliente: {ClienteId}, Usuario: {Usuario}, Items: {CantidadItems}",
                    request.ClienteId,
                    request.Usuario,
                    request.Items.Count
                );

                // Iniciar transacción
                await _orkestor.BeginTransactionAsync(cancellationToken);

                // Registrar inicio de proceso
                await _orkestor.LogAuditoria.RegistrarEventoAsync(
                    "PEDIDO_INICIADO",
                    $"Iniciando registro de pedido para cliente {request.ClienteId} por usuario {request.Usuario}"
                );
                await _orkestor.SaveChangesAsync(cancellationToken);

                // Calcular total
                decimal total = request.Items.Sum(item => item.Cantidad * item.Precio);

                _logger.LogInformation(
                    "Total calculado para pedido: {Total:C} ({CantidadItems} items)",
                    total,
                    request.Items.Count
                );

                // Validar con servicio externo
                _logger.LogInformation("Validando pedido con servicio externo...");
                bool esValido = await _validacionService.ValidarPedidoAsync(request.ClienteId, total);

                if (!esValido)
                {
                    await _orkestor.LogAuditoria.RegistrarEventoAsync(
                        "PEDIDO_RECHAZADO",
                        $"Pedido rechazado por servicio de validación. Cliente: {request.ClienteId}, Total: {total:C}, Usuario: {request.Usuario}"
                    );
                    await _orkestor.SaveChangesAsync(cancellationToken);
                    await _orkestor.RollbackTransactionAsync(cancellationToken);

                    _logger.LogWarning(
                        "Pedido rechazado - Cliente: {ClienteId}, Total: {Total:C}",
                        request.ClienteId,
                        total
                    );

                    return new PedidoResponse
                    {
                        Exito = false,
                        ClienteId = request.ClienteId,
                        Total = total,
                        Usuario = request.Usuario,
                        Mensaje = "El pedido no pudo ser validado. Verifique que el cliente exista y el monto no exceda el límite permitido."
                    };
                }

                // Crear pedido cabecera
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

                // Guardar en base de datos
                _logger.LogInformation("Guardando pedido en base de datos...");
                var pedidoCreado = await _orkestor.Pedidos.AddAsync(pedido);
                await _orkestor.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Pedido guardado exitosamente - PedidoId: {PedidoId}",
                    pedidoCreado.Id
                );

                // Registrar éxito
                await _orkestor.LogAuditoria.RegistrarEventoAsync(
                    "PEDIDO_CREADO",
                    $"Pedido {pedidoCreado.Id} creado exitosamente. Cliente: {request.ClienteId}, Total: {total:C}, Items: {request.Items.Count}, Usuario: {request.Usuario}"
                );
                await _orkestor.SaveChangesAsync(cancellationToken);

                // Confirmar transacción
                await _orkestor.CommitTransactionAsync(cancellationToken);

                _logger.LogInformation(
                    "Pedido registrado exitosamente - PedidoId: {PedidoId}, Cliente: {ClienteId}, Total: {Total:C}",
                    pedidoCreado.Id,
                    pedidoCreado.ClienteId,
                    pedidoCreado.Total
                );

                return new PedidoResponse
                {
                    Exito = true,
                    PedidoId = pedidoCreado.Id,
                    ClienteId = pedidoCreado.ClienteId,
                    Fecha = pedidoCreado.Fecha,
                    Total = pedidoCreado.Total,
                    Usuario = pedidoCreado.Usuario,
                    CantidadItems = request.Items.Count,
                    Mensaje = "Pedido registrado exitosamente"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al registrar pedido - Cliente: {ClienteId}, Usuario: {Usuario}",
                    request.ClienteId,
                    request.Usuario
                );

                // Rollback de la transacción
                await _orkestor.RollbackTransactionAsync(cancellationToken);

                // Intentar registrar el error en auditoría
                try
                {
                    await _orkestor.LogAuditoria.RegistrarEventoAsync(
                        "PEDIDO_ERROR",
                        $"Error al procesar pedido. Cliente: {request.ClienteId}, Usuario: {request.Usuario}, Error: {ex.Message}"
                    );
                    await _orkestor.SaveChangesAsync(cancellationToken);
                }
                catch (Exception logEx)
                {
                    _logger.LogError(logEx, "Error adicional al intentar registrar el error en auditoría");
                }

                throw new Exception($"Error al registrar pedido: {ex.Message}", ex);
            }
        }
    }
}