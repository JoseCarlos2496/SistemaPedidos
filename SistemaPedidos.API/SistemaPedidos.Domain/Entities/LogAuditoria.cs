namespace SistemaPedidos.Domain.Entities
{
    public class LogAuditoria
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public string Evento { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
    }
}