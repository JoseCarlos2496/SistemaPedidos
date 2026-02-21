using SistemaPedidos.Domain.Entities;
using SistemaPedidos.Domain.Interfaces;
using SistemaPedidos.Infrastructure.Data;

namespace SistemaPedidos.Infrastructure.Repositories
{
    /// <summary>
    /// Repositorio específico para entidad LogAuditoria.
    /// Proporciona método especializado para registrar eventos.
    /// </summary>
    /// <remarks>
    /// Extiende Repository genérico con método RegistrarEventoAsync.
    /// Usado por PedidoService para auditoría transaccional.
    /// </remarks>
    public class LogAuditoriaRepository : Repository<LogAuditoria>, ILogAuditoriaRepository
    {
        /// <summary>
        /// Constructor que pasa el contexto a la clase base.
        /// </summary>
        public LogAuditoriaRepository(SistemaPedidosDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Registra un evento de auditoría creando entrada en LogAuditoria.
        /// </summary>
        /// <remarks>
        /// Crea LogAuditoria con Fecha = DateTime.Now.
        /// NO persiste inmediatamente, requiere SaveChanges() del Orkestador.
        /// Se guarda en la misma transacción del pedido (Transactional Outbox).
        /// </remarks>
        /// <param name="evento">Nombre del evento (PEDIDO_INICIADO, PEDIDO_CREADO, etc.)</param>
        /// <param name="descripcion">Descripción detallada con IDs y contexto</param>
        public async Task RegistrarEventoAsync(string evento, string descripcion)
        {
            var log = new LogAuditoria
            {
                Evento = evento,
                Descripcion = descripcion,
                Fecha = DateTime.Now
            };

            await AddAsync(log);
        }
    }
}