using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Supermercado.Bodega.Models;
using Supermercado.Datos.Data;
using Supermercado.Datos.Models;

namespace Supermercado.Bodega.Controllers;

public class ProductosController(SupermercadoDbContext db) : Controller
{
    [HttpGet]
    public IActionResult Create() => View(new CrearProductoViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        CrearProductoViewModel model,
        CancellationToken cancellationToken)
    {
        model.Nombre = (model.Nombre ?? "").Trim();
        model.UnidadMedida = (model.UnidadMedida ?? "").Trim();

        if (await db.Productos.AnyAsync(p => p.Nombre == model.Nombre, cancellationToken))
        {
            ModelState.AddModelError(nameof(model.Nombre), "Ya existe un producto con ese nombre.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var producto = new Producto
        {
            Nombre = model.Nombre,
            UnidadMedida = model.UnidadMedida,
            PrecioUnitario = 0
        };
        db.Productos.Add(producto);
        await db.SaveChangesAsync(cancellationToken);

        TempData["Mensaje"] = $"Producto “{producto.Nombre}” creado con ID {producto.Id}.";
        return RedirectToAction("Index", "Bodegas");
    }
}
