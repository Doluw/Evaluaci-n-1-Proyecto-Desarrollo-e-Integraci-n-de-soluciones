using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Supermercado.Bodega.Models;

public class BodegaResumenViewModel
{
    public int Id { get; set; }
    public string Codigo { get; set; } = "";
    public string Nombre { get; set; } = "";
    public int ProductosDistintos { get; set; }
    public int UnidadesTotales { get; set; }
}

public class BodegaDetalleViewModel
{
    public int Id { get; set; }
    public string Codigo { get; set; } = "";
    public string Nombre { get; set; } = "";
    public IReadOnlyList<ExistenciaViewModel> Existencias { get; set; } = [];
}

public class ExistenciaViewModel
{
    public int ProductoId { get; set; }
    public string Producto { get; set; } = "";
    public string UnidadMedida { get; set; } = "";
    public int Cantidad { get; set; }
}

public class CrearBodegaViewModel
{
    [Required(ErrorMessage = "El código es obligatorio.")]
    [StringLength(30, MinimumLength = 2, ErrorMessage = "El código debe tener entre 2 y 30 caracteres.")]
    [RegularExpression("^[A-Za-z0-9_-]+$", ErrorMessage = "Use letras, números, guion o guion bajo.")]
    [Display(Name = "Código")]
    public string Codigo { get; set; } = "";

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres.")]
    public string Nombre { get; set; } = "";
}

public class CrearProductoViewModel
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(120, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 120 caracteres.")]
    public string Nombre { get; set; } = "";

    [Required(ErrorMessage = "La unidad de medida es obligatoria.")]
    [StringLength(30, ErrorMessage = "La unidad de medida admite hasta 30 caracteres.")]
    [Display(Name = "Unidad de medida")]
    public string UnidadMedida { get; set; } = "Unidad";
}

public class AjustarInventarioViewModel
{
    [Range(1, int.MaxValue, ErrorMessage = "Seleccione una bodega.")]
    [Display(Name = "Bodega")]
    public int BodegaId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Seleccione un producto.")]
    [Display(Name = "Producto")]
    public int ProductoId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor que cero.")]
    public int Cantidad { get; set; } = 1;

    [Required]
    public string Operacion { get; set; } = "agregar";

    public IEnumerable<SelectListItem> Bodegas { get; set; } = [];
    public IEnumerable<SelectListItem> Productos { get; set; } = [];
}

public class MoverInventarioViewModel
{
    [Range(1, int.MaxValue, ErrorMessage = "Seleccione la bodega de origen.")]
    [Display(Name = "Bodega de origen")]
    public int BodegaOrigenId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Seleccione la bodega de destino.")]
    [Display(Name = "Bodega de destino")]
    public int BodegaDestinoId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Seleccione un producto.")]
    [Display(Name = "Producto")]
    public int ProductoId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor que cero.")]
    public int Cantidad { get; set; } = 1;

    public IEnumerable<SelectListItem> Bodegas { get; set; } = [];
    public IEnumerable<SelectListItem> Productos { get; set; } = [];
}

public class ImportarJsonViewModel
{
    [Required(ErrorMessage = "Seleccione un archivo JSON.")]
    [Display(Name = "Archivo JSON")]
    public IFormFile? Archivo { get; set; }
}
