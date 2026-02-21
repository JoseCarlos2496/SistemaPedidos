using SistemaPedidos.Domain.Entities;

namespace SistemaPedidos.Domain.Interfaces
{
    public interface IPedidoRepository : IRepository<PedidoCabecera>
    {
        Task<PedidoCabecera?> ObtenerPedidoConDetallesAsync(int id);
        Task<IEnumerable<PedidoCabecera>> ObtenerPedidosPorClienteAsync(int clienteId);
    }
}