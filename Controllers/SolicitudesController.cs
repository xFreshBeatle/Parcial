using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Parcial.Data;
using Parcial.Models;

namespace Parcial.Controllers;

[Authorize]
public class SolicitudesController(ApplicationDbContext context) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] SolicitudFiltroViewModel filtros)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Challenge();
        }

        IQueryable<SolicitudCredito> query = context.SolicitudesCredito
            .AsNoTracking()
            .Include(s => s.Cliente)
            .Where(s => s.Cliente != null && s.Cliente.UsuarioId == userId);

        if (ModelState.IsValid)
        {
            if (filtros.Estado.HasValue)
            {
                query = query.Where(s => s.Estado == filtros.Estado.Value);
            }

            if (filtros.MontoMinimo.HasValue)
            {
                query = query.Where(s => s.MontoSolicitado >= filtros.MontoMinimo.Value);
            }

            if (filtros.MontoMaximo.HasValue)
            {
                query = query.Where(s => s.MontoSolicitado <= filtros.MontoMaximo.Value);
            }

            if (filtros.FechaInicio.HasValue)
            {
                var fechaInicio = filtros.FechaInicio.Value.Date;
                query = query.Where(s => s.FechaSolicitud >= fechaInicio);
            }

            if (filtros.FechaFin.HasValue)
            {
                var fechaFin = filtros.FechaFin.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(s => s.FechaSolicitud <= fechaFin);
            }
        }

        filtros.Resultados = await query
            .OrderByDescending(s => s.FechaSolicitud)
            .ThenByDescending(s => s.Id)
            .ToListAsync();

        return View(filtros);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Challenge();
        }

        var solicitud = await context.SolicitudesCredito
            .AsNoTracking()
            .Include(s => s.Cliente)
            .FirstOrDefaultAsync(s => s.Id == id && s.Cliente != null && s.Cliente.UsuarioId == userId);

        if (solicitud is null)
        {
            return NotFound();
        }

        return View(solicitud);
    }
}
