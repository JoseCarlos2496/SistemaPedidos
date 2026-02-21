namespace SistemaPedidos.Domain.Entities
{
    /// <summary>
    /// Entidad de dominio para registro de auditoría de eventos del sistema.
    /// Almacena trazabilidad completa de operaciones sobre pedidos.
    /// </summary>
    /// <remarks>
    /// Tabla: LogAuditoria
    /// Registra eventos: PEDIDO_INICIADO, PEDIDO_CREADO, PEDIDO_ERROR, etc.
    /// Guardado dentro de la misma transacción del pedido (Transactional Outbox).
    /// </remarks>
    public class LogAuditoria
    {
        /// <summary>
        /// Identificador único del registro de auditoría (clave primaria).
        /// Generado automáticamente por SQL Server (IDENTITY).
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Nombre del evento auditado.
        /// Ejemplos: PEDIDO_INICIADO, PEDIDO_CREADO, PEDIDO_ERROR, PEDIDO_CANCELADO.
        /// </summary>
        public string Evento { get; set; } = string.Empty;

        /// <summary>
        /// Descripción detallada del evento con contexto.
        /// Incluye IDs, totales, usuarios y mensajes relevantes.
        /// </summary>
        public string Descripcion { get; set; } = string.Empty;

        /// <summary>
        /// Fecha y hora de cuando ocurrió el evento.
        /// Timestamp del servidor (DateTime.Now o DateTime.UtcNow).
        /// </summary>
        public DateTime Fecha { get; set; }
    }
}