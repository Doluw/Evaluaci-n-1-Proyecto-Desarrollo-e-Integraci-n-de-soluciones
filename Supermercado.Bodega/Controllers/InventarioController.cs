using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Supermercado.Bodega.Models;
using Supermercado.Bodega.Services;
using Supermercado.Datos.Data;

namespace Supermercado.Bodega.Controllers;

public class InventarioController(
    SupermercadoDbContext db,
    InventarioService inventarioService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Ajustar(
        int? bodegaId,
        int? productoId,
        CancellationToken cancellationToken)
    {
        var model = new AjustarInventarioViewModel
        {
            BodegaId = bodegaId ?? 0,
            ProductoId = productoId ?? 0
        };
        await CargarOpcionesAsync(model, cancellationToken);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Ajustar(
        AjustarInventarioViewModel model,
        CancellationToken cancellationToken)
    {
        if (model.Operacion is not ("agregar" or "quitar"))
        {
            ModelState.AddModelError(nameof(model.Operacion), "Seleccione una operación válida.");
        }

        if (ModelState.IsValid)
        {
            try
            {
                await inventarioService.AjustarAsync(
                    model.BodegaId,
                    model.ProductoId,
                    model.Cantidad,
                    model.Operacion == "agregar",
                    cancellationToken);
                TempData["Mensaje"] = "Inventario actualizado correctamente.";
                return RedirectToAction("Details", "Bodegas", new { id = model.BodegaId });
            }
            catch (InventarioException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }
        }

        await CargarOpcionesAsync(model, cancellationToken);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Mover(
        int? bodegaId,
        int? productoId,
        CancellationToken cancellationToken)
    {
        var model = new MoverInventarioViewModel
        {
            BodegaOrigenId = bodegaId ?? 0,
            ProductoId = productoId ?? 0
        };
        await CargarOpcionesAsync(model, cancellationToken);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Mover(
        MoverInventarioViewModel model,
        CancellationToken cancellationToken)
    {
        if (ModelState.IsValid)
        {
            try
            {
                await inventarioService.MoverAsync(
                    model.BodegaOrigenId,
                    model.BodegaDestinoId,
                    model.ProductoId,
                    model.Cantidad,
                    cancellationToken);
                TempData["Mensaje"] = "Stock movido entre bodegas correctamente.";
                return RedirectToAction("Details", "Bodegas", new { id = model.BodegaDestinoId });
            }
            catch (InventarioException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }
        }

        await CargarOpcionesAsync(model, cancellationToken);
        return View(model);
    }

    [HttpGet]
    public IActionResult Importar() => View(new ImportarJsonViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(1_048_576)]
    public async Task<IActionResult> Importar(
        ImportarJsonViewModel model,
        CancellationToken cancellationToken)
    {
        if (model.Archivo is { Length: > 1_048_576 })
        {
            ModelState.AddModelError(nameof(model.Archivo), "El archivo no puede superar 1 MB.");
        }
        else if (model.Archivo is not null &&
            !string.Equals(Path.GetExtension(model.Archivo.FileName), ".json", StringComparison.OrdinalIgnoreCase))
        {
            ModelState.AddModelError(nameof(model.Archivo), "El archivo debe tener extensión .json.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            await using var stream = model.Archivo!.OpenReadStream();
            var items = await LeerJsonAsync(stream, cancellationToken);
            var registros = await inventarioService.ImportarAsync(items, cancellationToken);
            TempData["Mensaje"] = $"Importación completada: {registros} existencias actualizadas.";
            return RedirectToAction("Index", "Bodegas");
        }
        catch (JsonException)
        {
            ModelState.AddModelError(nameof(model.Archivo), "El contenido no es un JSON válido.");
        }
        catch (InventarioException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
        }

        return View(model);
    }

    private async Task CargarOpcionesAsync(
        AjustarInventarioViewModel model,
        CancellationToken cancellationToken)
    {
        model.Bodegas = await CrearOpcionesBodegasAsync(model.BodegaId, cancellationToken);
        model.Productos = await CrearOpcionesProductosAsync(model.ProductoId, cancellationToken);
    }

    private async Task CargarOpcionesAsync(
        MoverInventarioViewModel model,
        CancellationToken cancellationToken)
    {
        model.Bodegas = await CrearOpcionesBodegasAsync(0, cancellationToken);
        model.Productos = await CrearOpcionesProductosAsync(model.ProductoId, cancellationToken);
    }

    private async Task<List<SelectListItem>> CrearOpcionesBodegasAsync(
        int seleccionada,
        CancellationToken cancellationToken) =>
        await db.Bodegas.AsNoTracking().OrderBy(b => b.Nombre)
            .Select(b => new SelectListItem(b.Nombre + " (" + b.Codigo + ")", b.Id.ToString(), b.Id == seleccionada))
            .ToListAsync(cancellationToken);

    private async Task<List<SelectListItem>> CrearOpcionesProductosAsync(
        int seleccionado,
        CancellationToken cancellationToken) =>
        await db.Productos.AsNoTracking().OrderBy(p => p.Nombre)
            .Select(p => new SelectListItem(p.Nombre + " · " + p.UnidadMedida, p.Id.ToString(), p.Id == seleccionado))
            .ToListAsync(cancellationToken);

    private static async Task<List<ImportacionInventarioItem>> LeerJsonAsync(
        Stream stream,
        CancellationToken cancellationToken)
    {
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        var elementos = document.RootElement.ValueKind switch
        {
            JsonValueKind.Array => document.RootElement.EnumerateArray().ToList(),
            JsonValueKind.Object => [document.RootElement],
            _ => throw new JsonException()
        };

        return elementos.Select(elemento => new ImportacionInventarioItem(
            LeerEntero(elemento, "productoId"),
            LeerEntero(elemento, "bodegaId"),
            LeerEntero(elemento, "cantidad")))
            .ToList();
    }

    private static int LeerEntero(JsonElement elemento, string propiedad)
    {
        foreach (var item in elemento.EnumerateObject())
        {
            if (string.Equals(item.Name, propiedad, StringComparison.OrdinalIgnoreCase) &&
                item.Value.TryGetInt32(out var valor))
            {
                return valor;
            }
        }

        throw new JsonException();
    }
}
