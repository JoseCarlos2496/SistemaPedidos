using SistemaPedidos.Domain.Entities;
using SistemaPedidos.Domain.Interfaces;
using SistemaPedidos.Infrastructure.Data;

namespace SistemaPedidos.Infrastructure.Repositories
{
    public class LogAuditoriaRepository : Repository<LogAuditoria>, ILogAuditoriaRepository
    {
        public LogAuditoriaRepository(SistemaPedidosDbContext context) : base(context)
        {
        }

        public async Task RegistrarEventoAsync(string evento, string? descripcion)
        {
            var log = new LogAuditoria
            {
                Fecha = DateTime.Now,
                Evento = evento,
                Descripcion = descripcion
            };

            await AddAsync(log);
        }
    }
}