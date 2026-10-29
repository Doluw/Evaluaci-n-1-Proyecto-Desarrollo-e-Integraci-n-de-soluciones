using Microsoft.AspNetCore.Mvc;
using Supermercado.Ventas.Helpers;
using Supermercado.Ventas.Models;
using Supermercado.Ventas.Services;

namespace Supermercado.Ventas.Controllers;

public class VentasController : Controller
{
    private readonly VentaService _ventaService;

    public VentasController(VentaService ventaService)
    {
        _ventaService = ventaService;
    }

    // GET: /Ventas/Create
    public async Task<IActionResult> Create()
    {
        var modelo = new NuevaVentaViewModel
        {
            Productos = await _ventaService.ObtenerProductosVendiblesAsync()
        };
        return View(modelo);
    }

    // POST: /Ventas/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind(Prefix = "Input")] NuevaVentaInput input)
    {
        if (ModelState.IsValid)
        {
            var resultado = await _ventaService.RegistrarAsync(input);
            if (resultado.Exito)
            {
                TempData["Mensaje"] = $"Venta #{resultado.VentaId} registrada correctamente.";
                return RedirectToAction(nameof(Create));
            }

            foreach (var error in resultado.Errores)
            {
                ModelState.AddModelError(string.Empty, error);
            }
        }

        // Si hubo errores, se vuelve a mostrar el formulario con lo que ya se había ingresado.
        var modelo = new NuevaVentaViewModel
        {
            Input = input,
            Productos = await _ventaService.ObtenerProductosVendiblesAsync()
        };
        return View(modelo);
    }

    // GET: /Ventas/VerificarRut?rut=12345678-5
    // El formulario lo usa para avisar si el cliente tiene descuento.
    [HttpGet]
    public async Task<IActionResult> VerificarRut(string? rut)
    {
        if (!Rut.EsValido(rut))
        {
            return Json(new { valido = false, registrado = false });
        }

        var rutNormalizado = Rut.Normalizar(rut);
        var registrado = await _ventaService.ClienteRegistradoAsync(rutNormalizado);

        return Json(new
        {
            valido = true,
            registrado,
            rut = rutNormalizado,
            descuento = VentaService.PorcentajeDescuentoCliente
        });
    }
}