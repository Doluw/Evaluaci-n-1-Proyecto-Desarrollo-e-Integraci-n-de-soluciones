namespace Supermercado.Datos.Models;

public class DetalleVenta
{
    public int Id { get; set; }

    public int VentaId { get; set; }
    public Venta Venta { get; set; } = null!;

    public int ProductoId { get; set; }
    public Producto Producto { get; set; } = null!;

    public int Cantidad { get; set; }

    // Precio al momento de la venta (si después cambia el precio
    // del producto, la venta antigua no se altera).
    public decimal PrecioUnitario { get; set; }
}