namespace SistemaPedidos.Application.DTOs
{
    /// <summary>
    /// DTO de respuesta para pedido creado exitosamente.
    /// Solo se retorna en HTTP 201 Created. Los errores usan ErrorResponse.
    /// </summary>
    /// <remarks>
    /// Mapeado desde PedidoCabecera por PedidoService.CrearRespuestaExitosa().
    /// Desacopla entidad de dominio de contrato HTTP.
    /// NO incluye flag "Exito" ni mensaje (el código HTTP 201 indica éxito).
    /// </remarks>
    public class PedidoResponse
    {
        /// <summary>
        /// ID único del pedido generado por la base de datos (IDENTITY).
        /// Usado para consultas posteriores y referencias en otros sistemas.
        /// </summary>
        public int PedidoId { get; set; }

        /// <summary>
        /// ID del cliente que realizó el pedido.
        /// Validado previamente contra servicio externo.
        /// </summary>
        public int ClienteId { get; set; }

        /// <summary>
        /// Fecha y hora de creación del pedido (DateTime.Now del servidor).
        /// Formato ISO 8601 en JSON. Usar UTC en producción para sistemas distribuidos.
        /// </summary>
        public DateTime Fecha { get; set; }

        /// <summary>
        /// Total del pedido (suma de cantidad * precio de todos los items).
        /// Tipo decimal para precisión monetaria exacta. Sin símbolo de moneda.
        /// </summary>
        public decimal Total { get; set; }

        /// <summary>
        /// Usuario que registró el pedido en el sistema (vendedor/operador).
        /// Usado para trazabilidad, auditoría y comisiones.
        /// </summary>
        public string Usuario { get; set; } = string.Empty;

        /// <summary>
        /// Número de líneas/items en el pedido (no suma de cantidades).
        /// Ejemplo: 3 productos con cantidades 5, 2, 10 → CantidadItems = 3.
        /// </summary>
        public int CantidadItems { get; set; }
    }
}