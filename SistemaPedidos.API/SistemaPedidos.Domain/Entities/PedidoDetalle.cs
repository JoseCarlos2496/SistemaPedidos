namespace SistemaPedidos.Domain.Entities
{
    /// <summary>
    /// Entidad de dominio que representa una línea/item individual de un pedido.
    /// Contiene producto, cantidad y precio de una línea específica.
    /// </summary>
    /// <remarks>
    /// Tabla: PedidoDetalle
    /// Relación: N PedidoDetalle → 1 PedidoCabecera
    /// Id generado por SQL Server (IDENTITY).
    /// </remarks>
    public class PedidoDetalle
    {
        /// <summary>
        /// Identificador único del detalle (clave primaria).
        /// Generado automáticamente por SQL Server (IDENTITY).
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// ID del pedido al que pertenece este detalle (clave foránea).
        /// Relación con PedidoCabecera.
        /// </summary>
        public int PedidoId { get; set; }

        /// <summary>
        /// ID del producto en catálogo.
        /// Referencia a tabla Productos (si existe) o sistema externo.
        /// </summary>
        public int ProductoId { get; set; }

        /// <summary>
        /// Cantidad de unidades del producto en esta línea.
        /// Rango: 1 a 10,000 unidades.
        /// </summary>
        public int Cantidad { get; set; }

        /// <summary>
        /// Precio unitario del producto en esta línea.
        /// Tipo decimal para precisión monetaria.
        /// Permite precios diferentes para mismo producto (promociones).
        /// </summary>
        public decimal Precio { get; set; }

        /// <summary>
        /// Referencia de navegación a la cabecera del pedido.
        /// Propiedad de navegación de EF Core, puede ser null si no está cargada.
        /// </summary>
        public PedidoCabecera? PedidoCabecera { get; set; }
    }
}