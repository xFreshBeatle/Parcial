using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Parcial.Models;

public class SolicitudRegistroViewModel
{
    [Required(ErrorMessage = "Debe seleccionar un cliente.")]
    public int? ClienteId { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "El monto solicitado debe ser mayor a 0.")]
    public decimal MontoSolicitado { get; set; }

    public string? SuccessMessage { get; set; }

    public List<SelectListItem> Clientes { get; set; } = new();
}
