using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Supermercado.Ventas.Models;

// Fila del listado de precios.
public class ProductoPrecioItem
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string UnidadMedida { get; set; } = "";
    public decimal PrecioUnitario { get; set; }
    public int StockSalaVentas { get; set; }
}

// Formulario para cambiar el precio de un producto.
public class EditarPrecioViewModel
{
    public int Id { get; set; }

    [ValidateNever]
    public string Nombre { get; set; } = "";

    [ValidateNever]
    public string UnidadMedida { get; set; } = "";

    [Required(ErrorMessage = "Ingrese un precio.")]
    [Range(typeof(decimal), "1", "99999999", ErrorMessage = "El precio debe ser mayor a 0.")]
    [Display(Name = "Precio unitario")]
    public decimal? Precio { get; set; }
}