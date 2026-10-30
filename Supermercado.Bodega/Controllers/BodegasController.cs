using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Supermercado.Bodega.Models;
using Supermercado.Datos.Data;
using Supermercado.Datos.Models;

namespace Supermercado.Bodega.Controllers;

public class BodegasController(SupermercadoDbContext db) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var bodegas = await db.Bodegas
            .AsNoTracking()
            .OrderByDescending(b => b.Codigo == "VENTAS")
            .ThenBy(b => b.Nombre)
            .Select(b => new BodegaResumenViewModel
            {
                Id = b.Id,
                Codigo = b.Codigo,
                Nombre = b.Nombre,
                ProductosDistintos = b.Inventarios.Count(i => i.Cantidad > 0),
                UnidadesTotales = b.Inventarios.Sum(i => (int?)i.Cantidad) ?? 0
            })
            .ToListAsync(cancellationToken);

        return View(bodegas);
    }

    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var bodega = await db.Bodegas
            .AsNoTracking()
            .Where(b => b.Id == id)
            .Select(b => new BodegaDetalleViewModel
            {
                Id = b.Id,
                Codigo = b.Codigo,
                Nombre = b.Nombre,
                Existencias = b.Inventarios
                    .Where(i => i.Cantidad > 0)
                    .OrderBy(i => i.Producto.Nombre)
                    .Select(i => new ExistenciaViewModel
                    {
                        ProductoId = i.ProductoId,
                        Producto = i.Producto.Nombre,
                        UnidadMedida = i.Producto.UnidadMedida,
                        Cantidad = i.Cantidad
                    })
                    .ToList()
            })
            .SingleOrDefaultAsync(cancellationToken);

        return bodega is null ? NotFound() : View(bodega);
    }

    [HttpGet]
    public IActionResult Create() => View(new CrearBodegaViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        CrearBodegaViewModel model,
        CancellationToken cancellationToken)
    {
        model.Codigo = (model.Codigo ?? "").Trim().ToUpperInvariant();
        model.Nombre = (model.Nombre ?? "").Trim();

        if (await db.Bodegas.AnyAsync(b => b.Codigo == model.Codigo, cancellationToken))
        {
            ModelState.AddModelError(nameof(model.Codigo), "Ya existe una bodega con ese código.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        db.Bodegas.Add(new Supermercado.Datos.Models.Bodega
        {
            Codigo = model.Codigo,
            Nombre = model.Nombre
        });
        await db.SaveChangesAsync(cancellationToken);
        TempData["Mensaje"] = $"Bodega “{model.Nombre}” creada correctamente.";
        return RedirectToAction(nameof(Index));
    }
}
