using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Parcial.Data;
using Parcial.Models;

namespace Parcial.Controllers;

[Authorize(Roles = "Analista")]
[Route("Analista")]
public class AnalistaController(ApplicationDbContext context) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var pendientes = await context.SolicitudesCredito
            .AsNoTracking()
            .Include(s => s.Cliente)
            .Where(s => s.Estado == EstadoSolicitud.Pendiente)
            .OrderBy(s => s.FechaSolicitud)
            .ThenBy(s => s.Id)
            .ToListAsync();

        return View(pendientes);
    }

    [HttpPost("Aprobar/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Aprobar(int id)
    {
        var solicitud = await context.SolicitudesCredito
            .Include(s => s.Cliente)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (solicitud is null)
        {
            TempData["ErrorMessage"] = "La solicitud no existe.";
            return RedirectToAction(nameof(Index));
        }

        if (solicitud.Estado != EstadoSolicitud.Pendiente)
        {
            TempData["ErrorMessage"] = "No se pueden procesar solicitudes ya aprobadas o rechazadas.";
            return RedirectToAction(nameof(Index));
        }

        if (solicitud.Cliente is null)
        {
            TempData["ErrorMessage"] = "No se pudo encontrar el cliente asociado a la solicitud.";
            return RedirectToAction(nameof(Index));
        }

        if (solicitud.MontoSolicitado > solicitud.Cliente.IngresosMensuales * 5)
        {
            TempData["ErrorMessage"] = "No se puede aprobar: el monto excede 5 veces los ingresos del cliente.";
            return RedirectToAction(nameof(Index));
        }

        solicitud.Estado = EstadoSolicitud.Aprobado;
        solicitud.MotivoRechazo = null;

        await context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Solicitud #{solicitud.Id} aprobada correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("Rechazar/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Rechazar(int id, string? motivoRechazo)
    {
        if (string.IsNullOrWhiteSpace(motivoRechazo))
        {
            TempData["ErrorMessage"] = "El motivo de rechazo es obligatorio.";
            return RedirectToAction(nameof(Index));
        }

        var solicitud = await context.SolicitudesCredito
            .FirstOrDefaultAsync(s => s.Id == id);

        if (solicitud is null)
        {
            TempData["ErrorMessage"] = "La solicitud no existe.";
            return RedirectToAction(nameof(Index));
        }

        if (solicitud.Estado != EstadoSolicitud.Pendiente)
        {
            TempData["ErrorMessage"] = "No se pueden procesar solicitudes ya aprobadas o rechazadas.";
            return RedirectToAction(nameof(Index));
        }

        solicitud.Estado = EstadoSolicitud.Rechazado;
        solicitud.MotivoRechazo = motivoRechazo.Trim();

        await context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Solicitud #{solicitud.Id} rechazada correctamente.";
        return RedirectToAction(nameof(Index));
    }
}
