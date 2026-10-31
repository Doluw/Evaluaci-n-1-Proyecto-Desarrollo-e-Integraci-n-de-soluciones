using System.ComponentModel.DataAnnotations;

namespace Supermercado.Ventas.Models;

// Lo que llega desde el formulario de nueva venta.
public class NuevaVentaInput
{
    [Required(ErrorMessage = "Seleccione la caja.")]
    [Range(1, 10, ErrorMessage = "La caja debe estar entre 1 y 10.")]
    public int? Caja { get; set; }

    [Display(Name = "RUT cliente")]
    [Required(ErrorMessage = "Ingrese el RUT del cliente.")]
    [StringLength(12, ErrorMessage = "El RUT admite hasta 12 caracteres.")]
    public string? RutCliente { get; set; }

    public List<LineaVentaInput> Lineas { get; set; } = new();
}

// Un producto dentro de la venta.
public class LineaVentaInput
{
    public int ProductoId { get; set; }
    public int Cantidad { get; set; }
}

// Producto que se puede vender: tiene precio y stock en Sala de Ventas.
public class ProductoVendible
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string UnidadMedida { get; set; } = "";
    public decimal Precio { get; set; }
    public int Stock { get; set; }
}

// Datos que necesita la pantalla de nueva venta.
public class NuevaVentaViewModel
{
    public NuevaVentaInput Input { get; set; } = new();
    public List<ProductoVendible> Productos { get; set; } = new();
}


// Fila del listado de ventas.
public class VentaListItem
{
    public int Id { get; set; }
    public DateTime FechaHora { get; set; }
    public int Caja { get; set; }
    public string? RutCliente { get; set; }
    public bool ClienteRegistrado { get; set; }
    public decimal Descuento { get; set; }
    public decimal Total { get; set; }
    public int CantidadProductos { get; set; }
}

// Datos de la pantalla de listado (con los filtros aplicados).
public class ListadoVentasViewModel
{
    public List<VentaListItem> Ventas { get; set; } = new();
    public DateTime? Fecha { get; set; }
    public int? Caja { get; set; }
}
