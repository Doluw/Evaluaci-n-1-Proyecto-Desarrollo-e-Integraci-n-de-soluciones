namespace Supermercado.Datos.Models;

public class Inventario
{
    public int Id { get; set; }

    public int ProductoId { get; set; }
    public Producto Producto { get; set; } = null!;

    public int BodegaId { get; set; }
    public Bodega Bodega { get; set; } = null!;

    public int Cantidad { get; set; }
}