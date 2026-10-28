using System.ComponentModel.DataAnnotations;

namespace Supermercado.Datos.Models;

public class Bodega
{
    public int Id { get; set; }

    public string Codigo { get; set; } = "";

    public string Nombre { get; set; } = "";

    public ICollection<Inventario> Inventarios { get; set; }
        = new List<Inventario>();
}