using Microsoft.EntityFrameworkCore;
using Supermercado.Datos.Models;

namespace Supermercado.Datos.Data;

public class SupermercadoDbContext : DbContext
{
    public SupermercadoDbContext(
        DbContextOptions<SupermercadoDbContext> options)
        : base(options)
    {
    }

    public DbSet<Producto> Productos => Set<Producto>();

    public DbSet<Venta> Ventas => Set<Venta>();

    public DbSet<DetalleVenta> DetallesVenta => Set<DetalleVenta>();

    public DbSet<Bodega> Bodegas => Set<Bodega>();

    public DbSet<Inventario> Inventarios => Set<Inventario>();

    public DbSet<Cliente> Clientes => Set<Cliente>();

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Evitar códigos de bodega repetidos.
        modelBuilder.Entity<Bodega>()
            .HasIndex(b => b.Codigo)
            .IsUnique();

        // Evitar duplicar un producto en una misma bodega.
        modelBuilder.Entity<Inventario>()
            .HasIndex(i => new
            {
                i.BodegaId,
                i.ProductoId
            })
            .IsUnique();

        // Relación Inventario -> Bodega.
        modelBuilder.Entity<Inventario>()
            .HasOne(i => i.Bodega)
            .WithMany(b => b.Inventarios)
            .HasForeignKey(i => i.BodegaId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relación Inventario -> Producto.
        modelBuilder.Entity<Inventario>()
            .HasOne(i => i.Producto)
            .WithMany(p => p.Inventarios)
            .HasForeignKey(i => i.ProductoId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configurar precisión del precio.
        modelBuilder.Entity<Producto>()
            .Property(p => p.PrecioUnitario)
            .HasPrecision(18, 2);

        // ===== Ventas =====
        modelBuilder.Entity<Venta>(venta =>
        {
            venta.Property(v => v.RutCliente).HasMaxLength(12);
            venta.Property(v => v.Subtotal).HasPrecision(18, 2);
            venta.Property(v => v.PorcentajeDescuento).HasPrecision(5, 2);
            venta.Property(v => v.Descuento).HasPrecision(18, 2);
            venta.Property(v => v.Total).HasPrecision(18, 2);

            // Si se borra una venta, se borran sus detalles.
            venta.HasMany(v => v.Detalles)
                .WithOne(d => d.Venta)
                .HasForeignKey(d => d.VentaId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DetalleVenta>(detalle =>
        {
            detalle.Property(d => d.PrecioUnitario).HasPrecision(18, 2);

            // No se puede borrar un producto que ya fue vendido.
            detalle.HasOne(d => d.Producto)
                .WithMany()
                .HasForeignKey(d => d.ProductoId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ===== Clientes =====
        modelBuilder.Entity<Cliente>(cliente =>
        {
            cliente.Property(c => c.Rut).HasMaxLength(12).IsRequired();
            cliente.Property(c => c.Nombres).HasMaxLength(80).IsRequired();
            cliente.Property(c => c.Apellidos).HasMaxLength(80).IsRequired();
            cliente.Property(c => c.Email).HasMaxLength(120);
            cliente.Property(c => c.Telefono).HasMaxLength(20);

            // No pueden existir dos clientes con el mismo RUT.
            // Nota: Venta.RutCliente NO es FK a propósito, para que borrar
            // un cliente no altere el historial de ventas.
            cliente.HasIndex(c => c.Rut).IsUnique();
        });

        // Crear Sala de Ventas como bodega inicial.
        modelBuilder.Entity<Bodega>().HasData(
            new Bodega
            {
                Id = 1,
                Codigo = "VENTAS",
                Nombre = "Sala de Ventas"
            }
        );
    }
}
