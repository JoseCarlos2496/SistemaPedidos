using SistemaPedidos.Domain.Entities;

namespace SistemaPedidos.Domain.Interfaces
{
    /// <summary>
    /// Contrato para repositorio de pedidos (PedidoCabecera).
    /// Define operaciones CRUD sobre entidad PedidoCabecera.
    /// </summary>
    public interface IPedidoRepository
    {
        /// <summary>
        /// Agrega un nuevo pedido al contexto de Entity Framework.
        /// No persiste inmediatamente, requiere SaveChanges().
        /// </summary>
        /// <param name="pedido">Entidad PedidoCabecera con sus detalles</param>
        /// <returns>Entidad agregada al contexto (con Id = 0 hasta SaveChanges)</returns>
        Task<PedidoCabecera> AddAsync(PedidoCabecera pedido);
    }
}