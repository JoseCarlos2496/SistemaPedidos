namespace SistemaPedidos.Application.DTOs
{
    public class PedidoResponse
    {
        public bool Exito { get; set; }
        public int PedidoId { get; set; }
        public int ClienteId { get; set; }
        public DateTime Fecha { get; set; }
        public decimal Total { get; set; }
        public string Usuario { get; set; } = string.Empty;
        public string Mensaje { get; set; } = string.Empty;
        public int CantidadItems { get; set; }
    }
}