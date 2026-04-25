using System.ComponentModel.DataAnnotations;

namespace Parcial.Models;

public class SolicitudFiltroViewModel : IValidatableObject
{
    public EstadoSolicitud? Estado { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "El monto minimo no puede ser negativo.")]
    public decimal? MontoMinimo { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "El monto maximo no puede ser negativo.")]
    public decimal? MontoMaximo { get; set; }

    [DataType(DataType.Date)]
    public DateTime? FechaInicio { get; set; }

    [DataType(DataType.Date)]
    public DateTime? FechaFin { get; set; }

    public List<SolicitudCredito> Resultados { get; set; } = new();

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (FechaInicio.HasValue && FechaFin.HasValue && FechaInicio.Value.Date > FechaFin.Value.Date)
        {
            yield return new ValidationResult(
                "La fecha inicio no puede ser mayor a la fecha fin.",
                new[] { nameof(FechaInicio), nameof(FechaFin) });
        }

        if (MontoMinimo.HasValue && MontoMaximo.HasValue && MontoMinimo.Value > MontoMaximo.Value)
        {
            yield return new ValidationResult(
                "El monto minimo no puede ser mayor al monto maximo.",
                new[] { nameof(MontoMinimo), nameof(MontoMaximo) });
        }
    }
}
