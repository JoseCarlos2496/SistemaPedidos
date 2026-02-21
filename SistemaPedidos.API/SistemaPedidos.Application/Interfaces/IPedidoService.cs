using SistemaPedidos.Application.DTOs;

namespace SistemaPedidos.Application.Interfaces
{
    public interface IPedidoService
    {
        Task<PedidoResponse> RegistrarPedidoAsync(PedidoRequest request, CancellationToken cancellationToken = default);
    }
}