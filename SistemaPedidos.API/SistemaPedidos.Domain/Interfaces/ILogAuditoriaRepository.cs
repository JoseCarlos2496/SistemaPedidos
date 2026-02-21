using SistemaPedidos.Domain.Entities;

namespace SistemaPedidos.Domain.Interfaces
{
    public interface ILogAuditoriaRepository : IRepository<LogAuditoria>
    {
        Task RegistrarEventoAsync(string evento, string? descripcion);
    }
}