using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Supermercado.Clientes.Helpers;
using Supermercado.Clientes.Models;
using Supermercado.Datos.Data;
using Supermercado.Datos.Models;

namespace Supermercado.Clientes.Controllers;

public class ClientesController : Controller
{
    private readonly SupermercadoDbContext _context;

    public ClientesController(SupermercadoDbContext context)
    {
        _context = context;
    }

    // LISTAR (con búsqueda opcional por RUT, nombres o apellidos).
    public async Task<IActionResult> Index(string? q)
    {
        var query = _context.Clientes.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var texto = q.Trim();
            var rutBuscado = texto.Replace(".", "").Replace(" ", "").ToUpperInvariant();

            query = query.Where(c =>
                c.Rut.Contains(rutBuscado) ||
                c.Nombres.Contains(texto) ||
                c.Apellidos.Contains(texto));
        }

        ViewData["Busqueda"] = q;

        var clientes = await query
            .OrderBy(c => c.Apellidos)
            .ThenBy(c => c.Nombres)
            .ToListAsync();

        return View(clientes);
    }

    // AGREGAR
    public IActionResult Create()
    {
        return View(new ClienteViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ClienteViewModel vm)
    {
        if (!Rut.EsValido(vm.Rut))
        {
            ModelState.AddModelError(nameof(vm.Rut), "El RUT no es válido.");
        }

        var rut = Rut.Normalizar(vm.Rut);

        if (ModelState.IsValid &&
            await _context.Clientes.AnyAsync(c => c.Rut == rut))
        {
            ModelState.AddModelError(nameof(vm.Rut), "Ya existe un cliente con ese RUT.");
        }

        if (!ModelState.IsValid)
        {
            return View(vm);
        }

        _context.Clientes.Add(new Cliente
        {
            Rut = rut,
            Nombres = vm.Nombres.Trim(),
            Apellidos = vm.Apellidos.Trim(),
            Email = string.IsNullOrWhiteSpace(vm.Email) ? null : vm.Email.Trim(),
            Telefono = string.IsNullOrWhiteSpace(vm.Telefono) ? null : vm.Telefono.Trim(),
            FechaRegistro = DateTime.Now
        });

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // Respaldo por si otro usuario creó el mismo RUT justo antes
            // (el índice único de la BD lo rechaza).
            ModelState.AddModelError(nameof(vm.Rut), "Ya existe un cliente con ese RUT.");
            return View(vm);
        }

        TempData["Ok"] = "Cliente agregado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    // MODIFICAR
    public async Task<IActionResult> Edit(int id)
    {
        var cliente = await _context.Clientes.FindAsync(id);
        if (cliente == null) return NotFound();

        return View(new ClienteViewModel
        {
            Id = cliente.Id,
            Rut = cliente.Rut,
            Nombres = cliente.Nombres,
            Apellidos = cliente.Apellidos,
            Email = cliente.Email,
            Telefono = cliente.Telefono
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ClienteViewModel vm)
    {
        if (id != vm.Id) return NotFound();

        var cliente = await _context.Clientes.FindAsync(id);
        if (cliente == null) return NotFound();

        // El RUT no se modifica: Ventas lo usa para identificar al cliente.
        // Se ignora lo que llegue del formulario y se muestra el guardado.
        ModelState.Remove(nameof(vm.Rut));
        vm.Rut = cliente.Rut;

        if (!ModelState.IsValid)
        {
            return View(vm);
        }

        cliente.Nombres = vm.Nombres.Trim();
        cliente.Apellidos = vm.Apellidos.Trim();
        cliente.Email = string.IsNullOrWhiteSpace(vm.Email) ? null : vm.Email.Trim();
        cliente.Telefono = string.IsNullOrWhiteSpace(vm.Telefono) ? null : vm.Telefono.Trim();

        await _context.SaveChangesAsync();

        TempData["Ok"] = "Cliente actualizado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    // ELIMINAR (pide confirmación antes de borrar)
    public async Task<IActionResult> Delete(int id)
    {
        var cliente = await _context.Clientes
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);
        if (cliente == null) return NotFound();

        return View(cliente);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var cliente = await _context.Clientes.FindAsync(id);
        if (cliente != null)
        {
            _context.Clientes.Remove(cliente);
            await _context.SaveChangesAsync();
            TempData["Ok"] = "Cliente eliminado correctamente.";
        }

        return RedirectToAction(nameof(Index));
    }
}
