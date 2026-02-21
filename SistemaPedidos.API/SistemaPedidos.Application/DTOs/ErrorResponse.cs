namespace SistemaPedidos.Application.DTOs
{
    public class ErrorResponse
    {
        public string Mensaje { get; set; } = string.Empty;
        public string? Detalle { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public Dictionary<string, string[]>? Errores { get; set; }
    }
}