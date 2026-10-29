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
        // PENDIENTE: cuando Clientes agregue su entidad a Supermercado.Datos,
        // reemplazar la línea de abajo por:
        // return _context.Clientes.AnyAsync(c => c.Rut == rutNormalizado);
        return Task.FromResult(false);
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

        // Cliente (opcional).
        string? rut = null;
        bool clienteRegistrado = false;
        if (!string.IsNullOrWhiteSpace(input.RutCliente))
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
        // Un solo SaveChanges: EF guarda la venta, sus detalles y la rebaja
        // de stock en una misma transacción. Si algo falla, no se guarda nada.
        _context.Ventas.Add(venta);
        await _context.SaveChangesAsync();

        resultado.VentaId = venta.Id;
        return resultado;
    }
}