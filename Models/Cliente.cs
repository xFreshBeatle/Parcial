using System.ComponentModel.DataAnnotations;

namespace Parcial.Models;

public class Cliente
{
    public int Id { get; set; }

    [Required]
    public string UsuarioId { get; set; } = string.Empty;

    [Range(0.01, double.MaxValue)]
    public decimal IngresosMensuales { get; set; }

    public bool Activo { get; set; }

    public ICollection<SolicitudCredito> SolicitudesCredito { get; set; } = new List<SolicitudCredito>();
}
