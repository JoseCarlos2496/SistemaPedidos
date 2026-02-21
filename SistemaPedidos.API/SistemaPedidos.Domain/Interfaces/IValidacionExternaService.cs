namespace SistemaPedidos.Domain.Interfaces
{
    public interface IValidacionExternaService
    {
        Task<bool> ValidarPedidoAsync(int clienteId, decimal total);
    }
}