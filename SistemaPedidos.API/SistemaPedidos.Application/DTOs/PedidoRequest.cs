using System.ComponentModel.DataAnnotations;

namespace SistemaPedidos.Application.DTOs
{
    /// <summary>
    /// DTO de entrada para crear un nuevo pedido.
    /// Recibido en POST /api/pedidos desde body JSON.
    /// </summary>
    /// <remarks>
    /// Validado en 2 capas:
    /// 1. Data Annotations (ASP.NET Core automático) - formato y rangos básicos
    /// 2. PedidoService (lógica de negocio) - reglas complejas y contexto
    /// 
    /// Deserializado automáticamente por System.Text.Json (case-insensitive).
    /// </remarks>
    public class PedidoRequest
    {
        /// <summary>
        /// ID del cliente que realiza el pedido.
        /// Debe existir en servicio externo (JSONPlaceholder API). IDs válidos: 1-10.
        /// </summary>
        [Required(ErrorMessage = "El ClienteId es requerido")]
        [Range(1, int.MaxValue, ErrorMessage = "El ClienteId debe ser mayor a 0")]
        public int ClienteId { get; set; }

        /// <summary>
        /// Usuario que registra el pedido (vendedor/operador).
        /// Puede ser email, username o código de empleado. Longitud: 3-100 caracteres.
        /// NOTA DE SEGURIDAD: En producción debería obtenerse del token JWT, no del cliente.
        /// </summary>
        [Required(ErrorMessage = "El Usuario es requerido")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "El Usuario debe tener entre 3 y 100 caracteres")]
        public string Usuario { get; set; } = string.Empty;

        /// <summary>
        /// Lista de items/productos del pedido con cantidad y precio.
        /// Mínimo: 1 item, Máximo: 100 items (validado en PedidoService).
        /// Cada item genera un PedidoDetalle en BD.
        /// </summary>
        [Required(ErrorMessage = "Los Items son requeridos")]
        [MinLength(1, ErrorMessage = "Debe incluir al menos un item")]
        public List<PedidoItemRequest> Items { get; set; } = new();
    }
}