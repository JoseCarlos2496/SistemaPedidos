namespace SistemaPedidos.Domain.Entities
{
    /// <summary>
    /// Entidad de dominio que representa la cabecera/encabezado de un pedido.
    /// Contiene información principal del pedido y relación con sus detalles.
    /// </summary>
    /// <remarks>
    /// Tabla: PedidoCabecera
    /// Relación: 1 PedidoCabecera → N PedidoDetalle
    /// Id generado por SQL Server (IDENTITY).
    /// </remarks>
    public class PedidoCabecera
    {
        /// <summary>
        /// Identificador único del pedido (clave primaria).
        /// Generado automáticamente por SQL Server (IDENTITY).
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// ID del cliente que realizó el pedido.
        /// Referencia a sistema externo de clientes.
        /// </summary>
        public int ClienteId { get; set; }

        /// <summary>
        /// Fecha y hora de creación del pedido.
        /// Timestamp del servidor (DateTime.Now).
        /// </summary>
        public DateTime Fecha { get; set; }

        /// <summary>
        /// Total del pedido (suma de cantidad * precio de todos los detalles).
        /// Tipo decimal para precisión monetaria.
        /// </summary>
        public decimal Total { get; set; }

        /// <summary>
        /// Usuario que registró el pedido (vendedor/operador).
        /// Usado para trazabilidad y auditoría.
        /// </summary>
        public string Usuario { get; set; } = string.Empty;

        /// <summary>
        /// Colección de detalles/items del pedido (relación 1:N).
        /// Cada detalle es una línea con producto, cantidad y precio.
        /// Cargada por EF Core según configuración de navegación.
        /// </summary>
        public List<PedidoDetalle> Detalles { get; set; } = new();
    }
}