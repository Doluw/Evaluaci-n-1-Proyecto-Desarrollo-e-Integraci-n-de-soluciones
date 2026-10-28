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

    public DbSet<Bodega> Bodegas => Set<Bodega>();

    public DbSet<Inventario> Inventarios => Set<Inventario>();

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