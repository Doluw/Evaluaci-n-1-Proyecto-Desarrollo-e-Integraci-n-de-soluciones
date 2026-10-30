using System.ComponentModel.DataAnnotations;

namespace Supermercado.Clientes.Models;

public class ClienteViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El RUT es obligatorio.")]
    [StringLength(12)]
    [Display(Name = "RUT")]
    public string Rut { get; set; } = "";

    [Required(ErrorMessage = "Los nombres son obligatorios.")]
    [StringLength(80)]
    public string Nombres { get; set; } = "";

    [Required(ErrorMessage = "Los apellidos son obligatorios.")]
    [StringLength(80)]
    public string Apellidos { get; set; } = "";

    [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
    [StringLength(120)]
    [Display(Name = "Correo")]
    public string? Email { get; set; }

    [Phone(ErrorMessage = "El teléfono no tiene un formato válido.")]
    [StringLength(20)]
    [Display(Name = "Teléfono")]
    public string? Telefono { get; set; }
}
