using System.ComponentModel.DataAnnotations;

namespace SistemaPedidos.Application.DTOs
{
    /// <summary>
    /// DTO que representa un item/línea individual de un pedido.
    /// Contiene producto, cantidad y precio. Mapeado a PedidoDetalle.
    /// </summary>
    public class PedidoItemRequest
    {
        /// <summary>
        /// ID del producto en catálogo. Debe ser > 0.
        /// En sistema real se validaría existencia en tabla Productos.
        /// </summary>
        [Required(ErrorMessage = "El ProductoId es requerido")]
        [Range(1, int.MaxValue, ErrorMessage = "El ProductoId debe ser mayor a 0")]
        public int ProductoId { get; set; }

        /// <summary>
        /// Cantidad de unidades del producto.
        /// Rango validado en PedidoService: 1 a 10,000 unidades.
        /// </summary>
        [Required(ErrorMessage = "La Cantidad es requerida")]
        [Range(1, int.MaxValue, ErrorMessage = "La Cantidad debe ser mayor a 0")]
        public int Cantidad { get; set; }

        /// <summary>
        /// Precio unitario del producto.
        /// Rango validado en PedidoService: $0.01 a $999,999.99.
        /// Permite precios dinámicos/promocionales. Tipo decimal para precisión monetaria.
        /// </summary>
        [Required(ErrorMessage = "El Precio es requerido")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El Precio debe ser mayor a 0")]
        public decimal Precio { get; set; }
    }
}