namespace SistemaPedidos.Application.DTOs
{
    public class PedidoItemRequest
    {
        public int ProductoId { get; set; }
        public int Cantidad { get; set; }
        public decimal Precio { get; set; }
    }
}