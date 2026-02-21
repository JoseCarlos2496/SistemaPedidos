using Microsoft.EntityFrameworkCore;
using SistemaPedidos.Domain.Entities;
using SistemaPedidos.Domain.Interfaces;
using SistemaPedidos.Infrastructure.Data;

namespace SistemaPedidos.Infrastructure.Repositories
{
    /// <summary>
    /// Repositorio específico para entidad PedidoCabecera.
    /// Hereda operaciones CRUD de Repository genérico.
    /// </summary>
    /// <remarks>
    /// Actualmente solo usa métodos heredados.
    /// Se puede extender con queries específicas de pedidos (ej: GetByClienteIdAsync).
    /// </remarks>
    public class PedidoRepository : Repository<PedidoCabecera>, IPedidoRepository
    {
        /// <summary>
        /// Constructor que pasa el contexto a la clase base.
        /// </summary>
        public PedidoRepository(SistemaPedidosDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Agrega un pedido con sus detalles al contexto.
        /// </summary>
        /// <remarks>
        /// EF Core detecta automáticamente la relación y agrega los PedidoDetalle.
        /// Retorna la entidad agregada (con Id = 0 hasta SaveChanges).
        /// </remarks>
        public override async Task<PedidoCabecera> AddAsync(PedidoCabecera pedido)
        {
            return await base.AddAsync(pedido);
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