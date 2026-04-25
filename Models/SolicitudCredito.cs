using System.ComponentModel.DataAnnotations;

namespace Parcial.Models;

public enum EstadoSolicitud
{
    Pendiente = 0,
    Aprobado = 1,
    Rechazado = 2
}

public class SolicitudCredito
{
    public int Id { get; set; }

    [Required]
    public int ClienteId { get; set; }

    [Range(0.01, double.MaxValue)]
    public decimal MontoSolicitado { get; set; }

    public DateTime FechaSolicitud { get; set; }

    public EstadoSolicitud Estado { get; set; } = EstadoSolicitud.Pendiente;

    [MaxLength(250)]
    public string? MotivoRechazo { get; set; }

    public Cliente? Cliente { get; set; }
}
