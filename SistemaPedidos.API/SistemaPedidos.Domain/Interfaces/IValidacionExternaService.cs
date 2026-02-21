namespace SistemaPedidos.Domain.Interfaces
{
    /// <summary>
    /// Contrato para servicio de validación de pedidos contra servicios externos.
    /// Valida existencia de clientes y reglas de negocio externas.
    /// </summary>
    /// <remarks>
    /// Implementado por ValidacionExternaService en capa Infrastructure.
    /// Comunica con JSONPlaceholder API para verificar clientes.
    /// 
    /// En sistema real validaría contra:
    /// - Sistema de gestión de clientes (CRM)
    /// - Sistema de límites de crédito
    /// - Sistema de fraude/riesgo
    /// </remarks>
    public interface IValidacionExternaService
    {
        /// <summary>
        /// Valida un pedido contra reglas de negocio y servicios externos.
        /// </summary>
        /// <remarks>
        /// Validaciones ejecutadas:
        /// 1. ClienteId en rango válido (>= 1)
        /// 2. Total en rango válido (> 0)
        /// 3. Total no excede límite máximo configurado
        /// 4. Cliente existe en servicio externo (JSONPlaceholder)
        /// 
        /// Lanza excepciones específicas según tipo de error.
        /// NO retorna bool, todas las validaciones son throw or success.
        /// </remarks>
        /// <param name="clienteId">ID del cliente a validar</param>
        /// <param name="total">Total del pedido a validar</param>
        /// <exception cref="BusinessRuleException">Regla de negocio violada (cliente no existe, límite excedido)</exception>
        /// <exception cref="ExternalServiceException">Servicio externo no disponible o error de comunicación</exception>
        /// <exception cref="ConfigurationException">Error de configuración (URL, timeout, credenciales)</exception>
        Task ValidarPedidoAsync(int clienteId, decimal total);
    }
}