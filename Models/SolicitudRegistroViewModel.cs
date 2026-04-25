using System.ComponentModel.DataAnnotations;

namespace Parcial.Models;

public class SolicitudRegistroViewModel
{
    [Range(0.01, double.MaxValue, ErrorMessage = "El monto solicitado debe ser mayor a 0.")]
    public decimal MontoSolicitado { get; set; }

    public string? SuccessMessage { get; set; }
}
