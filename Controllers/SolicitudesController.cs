using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Parcial.Data;
using Parcial.Models;

namespace Parcial.Controllers;

[Authorize]
public class SolicitudesController(ApplicationDbContext context) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Challenge();
        }

        var model = new SolicitudRegistroViewModel
        {
            Clientes = await BuildClienteOptionsAsync(userId)
        };

        if (!model.Clientes.Any())
        {
            ModelState.AddModelError(string.Empty, "No tiene clientes activos para registrar solicitudes.");
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SolicitudRegistroViewModel model)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Challenge();
        }

        model.Clientes = await BuildClienteOptionsAsync(userId);

        if (!model.Clientes.Any())
        {
            ModelState.AddModelError(string.Empty, "No tiene clientes activos para registrar solicitudes.");
        }

        if (model.MontoSolicitado <= 0)
        {
            ModelState.AddModelError(nameof(model.MontoSolicitado), "El monto solicitado debe ser mayor a 0.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var cliente = await context.Clientes
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == model.ClienteId && c.UsuarioId == userId && c.Activo);

        if (cliente is null)
        {
            ModelState.AddModelError(string.Empty, "El cliente debe estar activo y pertenecer al usuario autenticado.");
            return View(model);
        }

        var existePendiente = await context.SolicitudesCredito
            .AnyAsync(s => s.ClienteId == cliente.Id && s.Estado == EstadoSolicitud.Pendiente);

        if (existePendiente)
        {
            ModelState.AddModelError(string.Empty, "El cliente ya tiene una solicitud en estado Pendiente.");
            return View(model);
        }

        var montoMaximoPermitido = cliente.IngresosMensuales * 10;
        if (model.MontoSolicitado > montoMaximoPermitido)
        {
            ModelState.AddModelError(
                nameof(model.MontoSolicitado),
                "El monto solicitado no puede superar 10 veces los ingresos mensuales del cliente.");
            return View(model);
        }

        var solicitud = new SolicitudCredito
        {
            ClienteId = cliente.Id,
            MontoSolicitado = model.MontoSolicitado,
            FechaSolicitud = DateTime.UtcNow,
            Estado = EstadoSolicitud.Pendiente
        };

        context.SolicitudesCredito.Add(solicitud);
        await context.SaveChangesAsync();

        var successModel = new SolicitudRegistroViewModel
        {
            ClienteId = cliente.Id,
            Clientes = await BuildClienteOptionsAsync(userId),
            SuccessMessage = $"Solicitud #{solicitud.Id} registrada exitosamente en estado Pendiente."
        };

        ModelState.Clear();
        return View(successModel);
    }

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

    private async Task<List<SelectListItem>> BuildClienteOptionsAsync(string userId)
    {
        return await context.Clientes
            .AsNoTracking()
            .Where(c => c.UsuarioId == userId && c.Activo)
            .OrderBy(c => c.Id)
            .Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = $"Cliente {c.Id} - Ingresos {c.IngresosMensuales:C}"
            })
            .ToListAsync();
    }
}
