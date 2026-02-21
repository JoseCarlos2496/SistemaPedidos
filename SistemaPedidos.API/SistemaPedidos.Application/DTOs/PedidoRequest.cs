using System.ComponentModel.DataAnnotations;

namespace SistemaPedidos.Application.DTOs
{
    public class PedidoRequest
    {
        [Required(ErrorMessage = "El ClienteId es requerido")]
        [Range(1, int.MaxValue, ErrorMessage = "El ClienteId debe ser mayor a 0")]
        public int ClienteId { get; set; }

        [Required(ErrorMessage = "El Usuario es requerido")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "El Usuario debe tener entre 3 y 100 caracteres")]
        public string Usuario { get; set; } = string.Empty;

        [Required(ErrorMessage = "Los Items son requeridos")]
        [MinLength(1, ErrorMessage = "Debe incluir al menos un item")]
        public List<PedidoItemRequest> Items { get; set; } = new();
    }
}