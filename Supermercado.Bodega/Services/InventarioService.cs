using System.Data;
using Microsoft.EntityFrameworkCore;
using Supermercado.Datos.Data;
using Supermercado.Datos.Models;

namespace Supermercado.Bodega.Services;

public sealed class InventarioService(SupermercadoDbContext db)
{
    public async Task AjustarAsync(
        int bodegaId,
        int productoId,
        int cantidad,
        bool agregar,
        CancellationToken cancellationToken = default)
    {
        if (cantidad <= 0)
        {
            throw new InventarioException("La cantidad debe ser mayor que cero.");
        }

        await using var transaction = await db.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);

        await ValidarReferenciasAsync(bodegaId, productoId, cancellationToken);

        var inventario = await db.Inventarios.SingleOrDefaultAsync(
            i => i.BodegaId == bodegaId && i.ProductoId == productoId,
            cancellationToken);

        if (agregar)
        {
            if (inventario is null)
            {
                db.Inventarios.Add(new Inventario
                {
                    BodegaId = bodegaId,
                    ProductoId = productoId,
                    Cantidad = cantidad
                });
            }
            else
            {
                inventario.Cantidad = SumarSinDesbordar(inventario.Cantidad, cantidad);
            }
        }
        else
        {
            if (inventario is null || inventario.Cantidad < cantidad)
            {
                throw new InventarioException("No hay stock suficiente para quitar esa cantidad.");
            }

            inventario.Cantidad -= cantidad;
            if (inventario.Cantidad == 0)
            {
                db.Inventarios.Remove(inventario);
            }
        }

        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    public async Task MoverAsync(
        int bodegaOrigenId,
        int bodegaDestinoId,
        int productoId,
        int cantidad,
        CancellationToken cancellationToken = default)
    {
        if (bodegaOrigenId == bodegaDestinoId)
        {
            throw new InventarioException("La bodega de origen y destino deben ser diferentes.");
        }

        if (cantidad <= 0)
        {
            throw new InventarioException("La cantidad debe ser mayor que cero.");
        }

        await using var transaction = await db.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);

        var bodegasValidas = await db.Bodegas.CountAsync(
            b => b.Id == bodegaOrigenId || b.Id == bodegaDestinoId,
            cancellationToken) == 2;
        if (!bodegasValidas || !await db.Productos.AnyAsync(p => p.Id == productoId, cancellationToken))
        {
            throw new InventarioException("La bodega o el producto seleccionado no existe.");
        }

        var filas = await db.Inventarios
            .Where(i => i.ProductoId == productoId &&
                (i.BodegaId == bodegaOrigenId || i.BodegaId == bodegaDestinoId))
            .ToListAsync(cancellationToken);

        var origen = filas.SingleOrDefault(i => i.BodegaId == bodegaOrigenId);
        if (origen is null || origen.Cantidad < cantidad)
        {
            throw new InventarioException("La bodega de origen no tiene stock suficiente.");
        }

        var destino = filas.SingleOrDefault(i => i.BodegaId == bodegaDestinoId);
        origen.Cantidad -= cantidad;
        if (origen.Cantidad == 0)
        {
            db.Inventarios.Remove(origen);
        }

        if (destino is null)
        {
            db.Inventarios.Add(new Inventario
            {
                BodegaId = bodegaDestinoId,
                ProductoId = productoId,
                Cantidad = cantidad
            });
        }
        else
        {
            destino.Cantidad = SumarSinDesbordar(destino.Cantidad, cantidad);
        }

        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    public async Task<int> ImportarAsync(
        IReadOnlyCollection<ImportacionInventarioItem> items,
        CancellationToken cancellationToken = default)
    {
        if (items.Count == 0)
        {
            throw new InventarioException("El archivo JSON no contiene registros.");
        }

        if (items.Any(i => i.ProductoId <= 0 || i.BodegaId <= 0 || i.Cantidad <= 0))
        {
            throw new InventarioException("Cada registro debe tener productoId, bodegaId y cantidad mayores que cero.");
        }

        var agrupados = items
            .GroupBy(i => new { i.BodegaId, i.ProductoId })
            .Select(g => new ImportacionInventarioItem(
                g.Key.ProductoId,
                g.Key.BodegaId,
                g.Aggregate(0, (total, item) => SumarSinDesbordar(total, item.Cantidad))))
            .ToList();

        await using var transaction = await db.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);

        var bodegaIds = agrupados.Select(i => i.BodegaId).Distinct().ToList();
        var productoIds = agrupados.Select(i => i.ProductoId).Distinct().ToList();
        var bodegasExistentes = await db.Bodegas
            .Where(b => bodegaIds.Contains(b.Id))
            .Select(b => b.Id)
            .ToListAsync(cancellationToken);
        var productosExistentes = await db.Productos
            .Where(p => productoIds.Contains(p.Id))
            .Select(p => p.Id)
            .ToListAsync(cancellationToken);

        var bodegaFaltante = bodegaIds.Except(bodegasExistentes).FirstOrDefault();
        var productoFaltante = productoIds.Except(productosExistentes).FirstOrDefault();
        if (bodegaFaltante != 0 || productoFaltante != 0)
        {
            var detalles = new List<string>();
            if (bodegaFaltante != 0) detalles.Add($"bodega {bodegaFaltante}");
            if (productoFaltante != 0) detalles.Add($"producto {productoFaltante}");
            throw new InventarioException($"No existe {string.Join(" ni ", detalles)}.");
        }

        var inventarios = await db.Inventarios
            .Where(i => bodegaIds.Contains(i.BodegaId) && productoIds.Contains(i.ProductoId))
            .ToListAsync(cancellationToken);

        foreach (var item in agrupados)
        {
            var inventario = inventarios.SingleOrDefault(i =>
                i.BodegaId == item.BodegaId && i.ProductoId == item.ProductoId);
            if (inventario is null)
            {
                db.Inventarios.Add(new Inventario
                {
                    BodegaId = item.BodegaId,
                    ProductoId = item.ProductoId,
                    Cantidad = item.Cantidad
                });
            }
            else
            {
                inventario.Cantidad = SumarSinDesbordar(inventario.Cantidad, item.Cantidad);
            }
        }

        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return agrupados.Count;
    }

    private async Task ValidarReferenciasAsync(
        int bodegaId,
        int productoId,
        CancellationToken cancellationToken)
    {
        if (!await db.Bodegas.AnyAsync(b => b.Id == bodegaId, cancellationToken) ||
            !await db.Productos.AnyAsync(p => p.Id == productoId, cancellationToken))
        {
            throw new InventarioException("La bodega o el producto seleccionado no existe.");
        }
    }

    private static int SumarSinDesbordar(int actual, int incremento)
    {
        try
        {
            return checked(actual + incremento);
        }
        catch (OverflowException)
        {
            throw new InventarioException("La cantidad resultante es demasiado grande.");
        }
    }
}

public sealed record ImportacionInventarioItem(int ProductoId, int BodegaId, int Cantidad);

public sealed class InventarioException(string message) : Exception(message);
