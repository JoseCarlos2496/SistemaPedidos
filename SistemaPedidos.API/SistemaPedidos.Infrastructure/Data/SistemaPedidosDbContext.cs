using Microsoft.EntityFrameworkCore;
using SistemaPedidos.Domain.Entities;

namespace SistemaPedidos.Infrastructure.Data
{
    public class SistemaPedidosDbContext : DbContext
    {
        public SistemaPedidosDbContext(DbContextOptions<SistemaPedidosDbContext> options)
            : base(options)
        {
        }

        public DbSet<PedidoCabecera> PedidoCabecera { get; set; }
        public DbSet<PedidoDetalle> PedidoDetalle { get; set; }
        public DbSet<LogAuditoria> LogAuditoria { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuración PedidoCabecera
            modelBuilder.Entity<PedidoCabecera>(entity =>
            {
                entity.ToTable("PedidoCabecera");
                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.Id)
                    .ValueGeneratedOnAdd();
                
                entity.Property(e => e.ClienteId)
                    .IsRequired();
                
                entity.Property(e => e.Fecha)
                    .IsRequired()
                    .HasDefaultValueSql("GETDATE()");
                
                entity.Property(e => e.Total)
                    .HasColumnType("decimal(18,2)")
                    .HasDefaultValue(0);
                
                entity.Property(e => e.Usuario)
                    .IsRequired()
                    .HasMaxLength(100);

                // Relación uno a muchos con PedidoDetalle
                entity.HasMany(e => e.Detalles)
                    .WithOne(d => d.Pedido)
                    .HasForeignKey(d => d.PedidoId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configuración PedidoDetalle
            modelBuilder.Entity<PedidoDetalle>(entity =>
            {
                entity.ToTable("PedidoDetalle");
                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.Id)
                    .ValueGeneratedOnAdd();
                
                entity.Property(e => e.PedidoId)
                    .IsRequired();
                
                entity.Property(e => e.ProductoId)
                    .IsRequired();
                
                entity.Property(e => e.Cantidad)
                    .IsRequired()
                    .HasDefaultValue(1);
                
                entity.Property(e => e.Precio)
                    .HasColumnType("decimal(18,2)")
                    .HasDefaultValue(0);
            });

            // Configuración LogAuditoria
            modelBuilder.Entity<LogAuditoria>(entity =>
            {
                entity.ToTable("LogAuditoria");
                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.Id)
                    .ValueGeneratedOnAdd();
                
                entity.Property(e => e.Fecha)
                    .IsRequired()
                    .HasDefaultValueSql("GETDATE()");
                
                entity.Property(e => e.Evento)
                    .IsRequired()
                    .HasMaxLength(200);
                
                entity.Property(e => e.Descripcion)
                    .HasMaxLength(4000);
            });
        }
    }
}