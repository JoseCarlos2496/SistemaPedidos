using SistemaPedidos.Application.DTOs;

namespace SistemaPedidos.Application.Interfaces
{
    /// <summary>
    /// Contrato para el servicio de gestión de pedidos.
    /// Define las operaciones de negocio disponibles para el módulo de pedidos.
    /// </summary>
    public interface IPedidoService
    {
        /// <summary>
        /// Registra un nuevo pedido en el sistema con validación, transaccionalidad y auditoría.
        /// </summary>
        /// <remarks>
        /// Proceso ejecutado:
        /// 1. Valida datos de entrada (formato y reglas de negocio)
        /// 2. Verifica existencia del cliente en servicio externo
        /// 3. Calcula y valida total del pedido
        /// 4. Persiste en base de datos dentro de transacción
        /// 5. Registra auditoría completa de la operación
        /// 
        /// En caso de error se ejecuta rollback automático.
        /// </remarks>
        /// <param name="request">Datos del pedido (cliente, usuario, items)</param>
        /// <param name="cancellationToken">Token de cancelación para operaciones asíncronas</param>
        /// <returns>PedidoResponse con ID generado, total calculado y datos del pedido</returns>
        /// <exception cref="ValidationException">Datos de entrada con formato inválido o fuera de rangos</exception>
        /// <exception cref="BusinessRuleException">Regla de negocio violada (cliente no existe, límite excedido)</exception>
        /// <exception cref="ExternalServiceException">Servicio de validación externa no disponible</exception>
        /// <exception cref="DatabaseException">Error al persistir en base de datos</exception>
        /// <exception cref="TransactionException">Error en manejo transaccional (inicio, commit, rollback)</exception>
        /// <exception cref="ConfigurationException">Error de configuración del sistema (estrategia, conexión)</exception>
        Task<PedidoResponse> RegistrarPedidoAsync(PedidoRequest request, CancellationToken cancellationToken = default);
    }
}