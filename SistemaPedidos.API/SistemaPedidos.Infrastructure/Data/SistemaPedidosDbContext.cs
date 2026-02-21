using Microsoft.EntityFrameworkCore;
using SistemaPedidos.Domain.Entities;

namespace SistemaPedidos.Infrastructure.Data
{
    /// <summary>
    /// Contexto de Entity Framework Core para acceso a base de datos.
    /// Define DbSets y configuración de entidades.
    /// </summary>
    /// <remarks>
    /// Configurado en Program.cs con:
    /// - SQL Server como proveedor
    /// - Estrategia de reintentos (3 intentos, 5 seg delay)
    /// - Connection string desde appsettings.json
    /// 
    /// Migraciones gestionadas con EF Core Tools:
    /// - Add-Migration NombreMigracion
    /// - Update-Database
    /// </remarks>
    public class SistemaPedidosDbContext : DbContext
    {
        /// <summary>
        /// Constructor que recibe opciones de configuración.
        /// </summary>
        public SistemaPedidosDbContext(DbContextOptions<SistemaPedidosDbContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// DbSet para tabla PedidoCabecera (pedidos).
        /// </summary>
        public DbSet<PedidoCabecera> PedidoCabecera { get; set; } = null!;

        /// <summary>
        /// DbSet para tabla PedidoDetalle (items de pedidos).
        /// </summary>
        public DbSet<PedidoDetalle> PedidoDetalle { get; set; } = null!;

        /// <summary>
        /// DbSet para tabla LogAuditoria (eventos de auditoría).
        /// </summary>
        public DbSet<LogAuditoria> LogAuditoria { get; set; } = null!;

        /// <summary>
        /// Configura el modelo de datos usando Fluent API.
        /// </summary>
        /// <remarks>
        /// Define:
        /// - Claves primarias (Id con IDENTITY)
        /// - Relaciones entre entidades (1:N PedidoCabecera-PedidoDetalle)
        /// - Restricciones (longitud strings, precisión decimales)
        /// - Índices para optimización de queries
        /// </remarks>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuración PedidoCabecera
            modelBuilder.Entity<PedidoCabecera>(entity =>
            {
                entity.ToTable("PedidoCabecera");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.ClienteId).IsRequired();
                entity.Property(e => e.Fecha).IsRequired();
                entity.Property(e => e.Total).HasPrecision(18, 2).IsRequired();
                entity.Property(e => e.Usuario).HasMaxLength(100).IsRequired();

                // Relación 1:N con PedidoDetalle
                entity.HasMany(e => e.Detalles)
                    .WithOne(d => d.PedidoCabecera)
                    .HasForeignKey(d => d.PedidoId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configuración PedidoDetalle
            modelBuilder.Entity<PedidoDetalle>(entity =>
            {
                entity.ToTable("PedidoDetalle");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.PedidoId).IsRequired();
                entity.Property(e => e.ProductoId).IsRequired();
                entity.Property(e => e.Cantidad).IsRequired();
                entity.Property(e => e.Precio).HasPrecision(18, 2).IsRequired();
            });

            // Configuración LogAuditoria
            modelBuilder.Entity<LogAuditoria>(entity =>
            {
                entity.ToTable("LogAuditoria");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.Evento).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Descripcion).HasMaxLength(500).IsRequired();
                entity.Property(e => e.Fecha).IsRequired();
            });
        }
    }
}