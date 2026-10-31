using System.Data;
using Microsoft.EntityFrameworkCore;
using Supermercado.Datos.Data;
using Supermercado.Datos.Models;
using Supermercado.Ventas.Helpers;
using Supermercado.Ventas.Models;

namespace Supermercado.Ventas.Services;

// Resultado de registrar una venta: el id creado o la lista de errores.
public class ResultadoVenta
{
    public int? VentaId { get; set; }
    public List<string> Errores { get; set; } = new();
    public bool Exito => VentaId.HasValue;
}

public class VentaService
{
    // Código de la bodega "Sala de Ventas" (migración inicial de Bodega).
    public const string CodigoSalaVentas = "VENTAS";

    // Descuento para clientes registrados.
    public const decimal PorcentajeDescuentoCliente = 2m;

    private readonly SupermercadoDbContext _context;

    public VentaService(SupermercadoDbContext context)
    {
        _context = context;
    }

    // Productos con precio asignado y stock en Sala de Ventas.
    public async Task<List<ProductoVendible>> ObtenerProductosVendiblesAsync()
    {
        return await _context.Inventarios
            .Where(i => i.Bodega.Codigo == CodigoSalaVentas
                     && i.Cantidad > 0
                     && i.Producto.PrecioUnitario > 0)
            .OrderBy(i => i.Producto.Nombre)
            .Select(i => new ProductoVendible
            {
                Id = i.ProductoId,
                Nombre = i.Producto.Nombre,
                UnidadMedida = i.Producto.UnidadMedida,
                Precio = i.Producto.PrecioUnitario,
                Stock = i.Cantidad
            })
            .ToListAsync();
    }

    // ¿El RUT corresponde a un cliente registrado?
    public Task<bool> ClienteRegistradoAsync(string rutNormalizado)
    {
        return _context.Clientes.AnyAsync(c => c.Rut == rutNormalizado);
    }

    // Nombre completo del cliente, o null si no está registrado.
    public async Task<string?> NombreClienteAsync(string? rut)
    {
        if (string.IsNullOrEmpty(rut)) return null;

        return await _context.Clientes
            .Where(c => c.Rut == rut)
            .Select(c => c.Nombres + " " + c.Apellidos)
            .FirstOrDefaultAsync();
    }

    public async Task<ResultadoVenta> RegistrarAsync(NuevaVentaInput input)
    {
        var resultado = new ResultadoVenta();

        // --- 1. Validaciones ---
        if (input.Caja is null or < 1 or > 10)
        {
            resultado.Errores.Add("La caja debe estar entre 1 y 10.");
        }

        // Si el mismo producto viene en varias líneas, se suman las cantidades.
        var lineas = input.Lineas
            .Where(l => l.ProductoId > 0)
            .GroupBy(l => l.ProductoId)
            .Select(g => new LineaVentaInput
            {
                ProductoId = g.Key,
                Cantidad = g.Sum(l => l.Cantidad)
            })
            .ToList();

        if (lineas.Count == 0)
        {
            resultado.Errores.Add("Agregue al menos un producto.");
        }

        // Cada venta debe identificar a su cliente. Si está registrado,
        // se aplica el descuento correspondiente.
        string? rut = null;
        bool clienteRegistrado = false;
        if (string.IsNullOrWhiteSpace(input.RutCliente))
        {
            resultado.Errores.Add("Ingrese el RUT del cliente.");
        }
        else
        {
            if (!Rut.EsValido(input.RutCliente))
            {
                resultado.Errores.Add("El RUT ingresado no es válido.");
            }
            else
            {
                rut = Rut.Normalizar(input.RutCliente);
                clienteRegistrado = await ClienteRegistradoAsync(rut);
            }
        }

        if (resultado.Errores.Count > 0)
        {
            return resultado;
        }

        // SERIALIZABLE evita que dos cajas vendan simultáneamente las
        // mismas unidades de inventario.
        await using var transaction = await _context.Database.BeginTransactionAsync(
            IsolationLevel.Serializable);

        // Inventario de Sala de Ventas de los productos vendidos.
        var ids = lineas.Select(l => l.ProductoId).ToList();
        var inventarios = await _context.Inventarios
            .Include(i => i.Producto)
            .Where(i => i.Bodega.Codigo == CodigoSalaVentas && ids.Contains(i.ProductoId))
            .ToDictionaryAsync(i => i.ProductoId);

        foreach (var linea in lineas)
        {
            if (!inventarios.TryGetValue(linea.ProductoId, out var inventario))
            {
                resultado.Errores.Add($"El producto #{linea.ProductoId} no está en la Sala de Ventas.");
                continue;
            }

            var nombre = inventario.Producto.Nombre;

            if (linea.Cantidad <= 0)
            {
                resultado.Errores.Add($"{nombre}: la cantidad debe ser mayor a 0.");
            }
            else if (linea.Cantidad > inventario.Cantidad)
            {
                resultado.Errores.Add($"{nombre}: stock insuficiente (disponible {inventario.Cantidad}).");
            }

            if (inventario.Producto.PrecioUnitario <= 0)
            {
                resultado.Errores.Add($"{nombre}: no tiene precio asignado.");
            }
        }

        if (resultado.Errores.Count > 0)
        {
            return resultado;
        }

        // --- 2. Crear la venta y rebajar el stock ---
        var venta = new Venta
        {
            FechaHora = DateTime.Now,
            Caja = input.Caja!.Value,
            RutCliente = rut,
            ClienteRegistrado = clienteRegistrado
        };

        foreach (var linea in lineas)
        {
            var inventario = inventarios[linea.ProductoId];

            venta.Detalles.Add(new DetalleVenta
            {
                ProductoId = linea.ProductoId,
                Cantidad = linea.Cantidad,
                // Se copia el precio actual: si después cambia, esta venta no se altera.
                PrecioUnitario = inventario.Producto.PrecioUnitario
            });

            inventario.Cantidad -= linea.Cantidad;
        }

        // --- 3. Montos (pesos chilenos, sin decimales) ---
        venta.Subtotal = venta.Detalles.Sum(d => d.Cantidad * d.PrecioUnitario);
        venta.PorcentajeDescuento = clienteRegistrado ? PorcentajeDescuentoCliente : 0;
        venta.Descuento = Math.Round(
            venta.Subtotal * venta.PorcentajeDescuento / 100, 0, MidpointRounding.AwayFromZero);
        venta.Total = venta.Subtotal - venta.Descuento;

        // --- 4. Guardar ---
        // La venta, sus detalles y la rebaja de stock quedan en la misma
        // transacción. Si la existencia cambió, no se guarda una venta parcial.
        _context.Ventas.Add(venta);
        try
        {
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (DbUpdateException)
        {
            await transaction.RollbackAsync();
            resultado.Errores.Add(
                "No fue posible registrar la venta porque el inventario cambió. Intente nuevamente.");
            return resultado;
        }

        resultado.VentaId = venta.Id;
        return resultado;
    }

    // Listado de ventas, con filtros opcionales por fecha y caja.
    public async Task<List<VentaListItem>> ListarAsync(DateTime? fecha, int? caja)
    {
        var consulta = _context.Ventas.AsQueryable();

        if (fecha.HasValue)
        {
            var desde = fecha.Value.Date;
            var hasta = desde.AddDays(1);
            consulta = consulta.Where(v => v.FechaHora >= desde && v.FechaHora < hasta);
        }

        if (caja.HasValue)
        {
            consulta = consulta.Where(v => v.Caja == caja.Value);
        }

        return await consulta
            .OrderByDescending(v => v.FechaHora)
            .Select(v => new VentaListItem
            {
                Id = v.Id,
                FechaHora = v.FechaHora,
                Caja = v.Caja,
                RutCliente = v.RutCliente,
                ClienteRegistrado = v.ClienteRegistrado,
                Descuento = v.Descuento,
                Total = v.Total,
                CantidadProductos = v.Detalles.Count
            })
            .ToListAsync();
    }

    // Una venta con sus productos (para la pantalla de detalle).
    public async Task<Venta?> ObtenerAsync(int id)
    {
        return await _context.Ventas
            .Include(v => v.Detalles)
                .ThenInclude(d => d.Producto)
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.Id == id);
    }
}
