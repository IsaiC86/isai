
using Microsoft.EntityFrameworkCore;
using Intermediario.Models;

namespace Intermediario.Data
{
    public class MarketplaceContext : DbContext
    {
        public MarketplaceContext(
            DbContextOptions<MarketplaceContext> options)
            : base(options)
        {
        }

        // Tablas de la base de datos
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Producto> Productos { get; set; }
        public DbSet<ProductoImagen> ProductoImagenes { get; set; }
        public DbSet<Transaccion> Transacciones { get; set; }
        public DbSet<Disputa> Disputas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Producto - Vendedor
            modelBuilder.Entity<Producto>()
                .HasOne(p => p.Vendedor)
                .WithMany()
                .HasForeignKey(p => p.VendedorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Producto - Imágenes
            modelBuilder.Entity<ProductoImagen>()
                .HasOne(i => i.Producto)
                .WithMany(p => p.Imagenes)
                .HasForeignKey(i => i.ProductoId)
                .OnDelete(DeleteBehavior.Cascade);

            // Transacción - Producto
            modelBuilder.Entity<Transaccion>()
                .HasOne(t => t.Producto)
                .WithMany()
                .HasForeignKey(t => t.producto_id)
                .OnDelete(DeleteBehavior.Restrict);

            // Transacción - Comprador
            modelBuilder.Entity<Transaccion>()
                .HasOne(t => t.Comprador)
                .WithMany()
                .HasForeignKey(t => t.comprador_id)
                .OnDelete(DeleteBehavior.Restrict);

            // Transacción - Vendedor
            modelBuilder.Entity<Transaccion>()
                .HasOne(t => t.Vendedor)
                .WithMany()
                .HasForeignKey(t => t.vendedor_id)
                .OnDelete(DeleteBehavior.Restrict);

            // Disputa - Transacción
            modelBuilder.Entity<Disputa>()
                .HasOne(d => d.Transaccion)
                .WithMany()
                .HasForeignKey(d => d.transaccion_id)
                .OnDelete(DeleteBehavior.Restrict);

            // Disputa - Usuario que reporta
            modelBuilder.Entity<Disputa>()
                .HasOne(d => d.UsuarioReporta)
                .WithMany()
                .HasForeignKey(d => d.usuario_reporta_id)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
