using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Supermercado.Datos.Data;
using Supermercado.Ventas.Models;

namespace Supermercado.Ventas.Controllers;

public class ProductosController : Controller
{
    // Código de la bodega "Sala de Ventas" (creada en la migración inicial).
    private const string CodigoSalaVentas = "VENTAS";

    private readonly SupermercadoDbContext _context;

    public ProductosController(SupermercadoDbContext context)
    {
        _context = context;
    }

    // GET: /Productos
    public async Task<IActionResult> Index(string? buscar)
    {
        var consulta = _context.Productos.AsQueryable();

        if (!string.IsNullOrWhiteSpace(buscar))
        {
            consulta = consulta.Where(p => p.Nombre.Contains(buscar));
        }

        var productos = await consulta
            .OrderBy(p => p.Nombre)
            .Select(p => new ProductoPrecioItem
            {
                Id = p.Id,
                Nombre = p.Nombre,
                UnidadMedida = p.UnidadMedida,
                PrecioUnitario = p.PrecioUnitario,
                StockSalaVentas = p.Inventarios
                    .Where(i => i.Bodega.Codigo == CodigoSalaVentas)
                    .Sum(i => i.Cantidad)
            })
            .ToListAsync();

        ViewData["Buscar"] = buscar;
        return View(productos);
    }

    // GET: /Productos/EditarPrecio/5
    public async Task<IActionResult> EditarPrecio(int id)
    {
        var producto = await _context.Productos.FindAsync(id);
        if (producto == null)
        {
            return NotFound();
        }

        var modelo = new EditarPrecioViewModel
        {
            Id = producto.Id,
            Nombre = producto.Nombre,
            UnidadMedida = producto.UnidadMedida,
            Precio = producto.PrecioUnitario > 0 ? producto.PrecioUnitario : null
        };

        return View(modelo);
    }

    // POST: /Productos/EditarPrecio/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditarPrecio(int id, EditarPrecioViewModel modelo)
    {
        var producto = await _context.Productos.FindAsync(id);
        if (producto == null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            modelo.Id = producto.Id;
            modelo.Nombre = producto.Nombre;
            modelo.UnidadMedida = producto.UnidadMedida;
            return View(modelo);
        }

        // Pesos chilenos: sin decimales.
        // Cambiar el precio no afecta ventas anteriores, porque cada
        // DetalleVenta guarda su propio precio unitario.
        producto.PrecioUnitario = Math.Round(modelo.Precio!.Value, 0);
        await _context.SaveChangesAsync();

        TempData["Mensaje"] = $"Precio de \"{producto.Nombre}\" actualizado.";
        return RedirectToAction(nameof(Index));
    }
}