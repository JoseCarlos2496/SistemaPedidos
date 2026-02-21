using SistemaPedidos.Domain.Entities;

namespace SistemaPedidos.Domain.Interfaces
{
    /// <summary>
    /// Contrato para repositorio de auditoría.
    /// Define operaciones para registro de eventos del sistema.
    /// </summary>
    public interface ILogAuditoriaRepository
    {
        /// <summary>
        /// Registra un evento de auditoría en el sistema.
        /// Crea entrada en LogAuditoria con timestamp actual.
        /// No persiste inmediatamente, requiere SaveChanges().
        /// </summary>
        /// <param name="evento">Nombre del evento (PEDIDO_INICIADO, PEDIDO_CREADO, etc.)</param>
        /// <param name="descripcion">Descripción detallada con contexto e IDs relevantes</param>
        Task RegistrarEventoAsync(string evento, string descripcion);
    }
}