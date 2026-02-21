using Microsoft.EntityFrameworkCore;
using SistemaPedidos.Domain.Entities;
using SistemaPedidos.Domain.Interfaces;
using SistemaPedidos.Infrastructure.Data;

namespace SistemaPedidos.Infrastructure.Repositories
{
    public class PedidoRepository : Repository<PedidoCabecera>, IPedidoRepository
    {
        public PedidoRepository(SistemaPedidosDbContext context) : base(context)
        {
        }

        public async Task<PedidoCabecera?> ObtenerPedidoConDetallesAsync(int id)
        {
            return await _dbSet
                .Include(p => p.Detalles)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<PedidoCabecera>> ObtenerPedidosPorClienteAsync(int clienteId)
        {
            return await _dbSet
                .Include(p => p.Detalles)
                .Where(p => p.ClienteId == clienteId)
                .OrderByDescending(p => p.Fecha)
                .ToListAsync();
        }
    }
}