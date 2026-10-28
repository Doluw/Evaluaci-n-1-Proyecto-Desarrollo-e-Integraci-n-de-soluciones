using System.ComponentModel.DataAnnotations;

namespace Supermercado.Datos.Models;

public class Producto
{
    public int Id { get; set; }

    public string Nombre { get; set; } = "";

    public string UnidadMedida { get; set; } = "";

    public decimal PrecioUnitario { get; set; }

    public ICollection<Inventario> Inventarios { get; set; }
        = new List<Inventario>();
}