namespace Supermercado.Datos.Models;

public class Cliente
{
    public int Id { get; set; }

    // RUT normalizado, sin puntos y con guion: "12345678-K".
    // Es la clave natural que usa Ventas (Venta.RutCliente).
    public string Rut { get; set; } = "";

    public string Nombres { get; set; } = "";

    public string Apellidos { get; set; } = "";

    public string? Email { get; set; }

    public string? Telefono { get; set; }

    public DateTime FechaRegistro { get; set; }
}
