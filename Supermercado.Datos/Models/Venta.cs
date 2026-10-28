namespace Supermercado.Datos.Models;

public class Venta
{
    public int Id { get; set; }

    // Fecha y hora en que se registró la venta.
    public DateTime FechaHora { get; set; }

    // Número de caja (1 a 10).
    public int Caja { get; set; }

    // RUT del cliente (null si no se ingresó).
    public string? RutCliente { get; set; }

    // Indica si el cliente estaba registrado al momento de la compra.
    public bool ClienteRegistrado { get; set; }

    public decimal Subtotal { get; set; }

    // 2 si el cliente estaba registrado, 0 si no.
    public decimal PorcentajeDescuento { get; set; }

    public decimal Descuento { get; set; }

    public decimal Total { get; set; }

    public ICollection<DetalleVenta> Detalles { get; set; }
        = new List<DetalleVenta>();
}