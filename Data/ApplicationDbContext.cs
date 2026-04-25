using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Parcial.Models;

namespace Parcial.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<IdentityUser>(options)
{
	private const string AnalistaRoleId = "4f6fd1d9-b1ef-4200-9827-6ce304f2b001";
	private const string AnalistaUserId = "c31ccfbc-df70-456b-9666-4f083ec3f08e";

	public DbSet<Cliente> Clientes => Set<Cliente>();
	public DbSet<SolicitudCredito> SolicitudesCredito => Set<SolicitudCredito>();

	protected override void OnModelCreating(ModelBuilder builder)
	{
		base.OnModelCreating(builder);

		SeedIdentity(builder);

		builder.Entity<Cliente>(entity =>
		{
			entity.Property(c => c.IngresosMensuales).HasPrecision(18, 2);
			entity.HasOne<IdentityUser>()
				.WithMany()
				.HasForeignKey(c => c.UsuarioId)
				.OnDelete(DeleteBehavior.Restrict);

			entity.ToTable(t => t.HasCheckConstraint("CK_Clientes_IngresosMensuales", "IngresosMensuales > 0"));

			entity.HasData(
				new Cliente
				{
					Id = 1,
					UsuarioId = AnalistaUserId,
					IngresosMensuales = 1000m,
					Activo = true
				},
				new Cliente
				{
					Id = 2,
					UsuarioId = AnalistaUserId,
					IngresosMensuales = 2500m,
					Activo = true
				});
		});

		builder.Entity<SolicitudCredito>(entity =>
		{
			entity.Property(s => s.MontoSolicitado).HasPrecision(18, 2);

			entity.HasOne(s => s.Cliente)
				.WithMany(c => c.SolicitudesCredito)
				.HasForeignKey(s => s.ClienteId)
				.OnDelete(DeleteBehavior.Cascade);

			entity.ToTable(t => t.HasCheckConstraint("CK_SolicitudesCredito_MontoSolicitado", "MontoSolicitado > 0"));

			entity.HasIndex(s => s.ClienteId)
				.IsUnique()
				.HasFilter($"{nameof(SolicitudCredito.Estado)} = {(int)EstadoSolicitud.Pendiente}");

			entity.HasData(
				new SolicitudCredito
				{
					Id = 1,
					ClienteId = 1,
					MontoSolicitado = 800m,
					FechaSolicitud = new DateTime(2026, 4, 1),
					Estado = EstadoSolicitud.Pendiente,
					MotivoRechazo = null
				},
				new SolicitudCredito
				{
					Id = 2,
					ClienteId = 2,
					MontoSolicitado = 2000m,
					FechaSolicitud = new DateTime(2026, 4, 2),
					Estado = EstadoSolicitud.Aprobado,
					MotivoRechazo = null
				});
		});
	}

	private static void SeedIdentity(ModelBuilder builder)
	{
		var role = new IdentityRole
		{
			Id = AnalistaRoleId,
			Name = "Analista",
			NormalizedName = "ANALISTA",
			ConcurrencyStamp = "f9c55b8a-3c88-4b03-9193-cfc6beb8ebbc"
		};

		var user = new IdentityUser
		{
			Id = AnalistaUserId,
			UserName = "analista@parcial.local",
			NormalizedUserName = "ANALISTA@PARCIAL.LOCAL",
			Email = "analista@parcial.local",
			NormalizedEmail = "ANALISTA@PARCIAL.LOCAL",
			EmailConfirmed = true,
			SecurityStamp = "EAAE2E2A-A58F-4B08-B9EE-E06D9EA82185",
			ConcurrencyStamp = "6D71E2FC-A9BD-4186-81D0-6AC7A9E85683"
		};

		user.PasswordHash = "AQAAAAIAAYagAAAAEIJrgXtc5St3z4Z5kh9XpgeSk6nQvrj6yqNPT0R7LIa/73TLwZNLFAbojebTtOLung==";

		builder.Entity<IdentityRole>().HasData(role);
		builder.Entity<IdentityUser>().HasData(user);
		builder.Entity<IdentityUserRole<string>>().HasData(new IdentityUserRole<string>
		{
			UserId = AnalistaUserId,
			RoleId = AnalistaRoleId
		});
	}

	public override int SaveChanges()
	{
		ValidateSolicitudReglasDeNegocio();
		return base.SaveChanges();
	}

	public override int SaveChanges(bool acceptAllChangesOnSuccess)
	{
		ValidateSolicitudReglasDeNegocio();
		return base.SaveChanges(acceptAllChangesOnSuccess);
	}

	public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
	{
		ValidateSolicitudReglasDeNegocio();
		return base.SaveChangesAsync(cancellationToken);
	}

	public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
	{
		ValidateSolicitudReglasDeNegocio();
		return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
	}

	private void ValidateSolicitudReglasDeNegocio()
	{
		var ingresosByCliente = ChangeTracker.Entries<Cliente>()
			.Where(e => e.State is EntityState.Added or EntityState.Modified)
			.Select(e => e.Entity)
			.ToDictionary(c => c.Id, c => c.IngresosMensuales);

		var entries = ChangeTracker.Entries<SolicitudCredito>()
			.Where(e => e.State is EntityState.Added or EntityState.Modified)
			.Select(e => e.Entity)
			.Where(s => s.Estado == EstadoSolicitud.Aprobado)
			.ToList();

		foreach (var solicitud in entries)
		{
			var ingresosMensuales = ingresosByCliente.TryGetValue(solicitud.ClienteId, out var ingresos)
				? ingresos
				: Clientes
					.Where(c => c.Id == solicitud.ClienteId)
					.Select(c => c.IngresosMensuales)
					.FirstOrDefault();

			if (ingresosMensuales <= 0)
			{
				throw new InvalidOperationException("El cliente asociado no existe o no tiene ingresos mensuales validos.");
			}

			if (solicitud.MontoSolicitado > ingresosMensuales * 5)
			{
				throw new InvalidOperationException("No se puede aprobar una solicitud cuyo monto sea mayor a 5 veces los ingresos mensuales del cliente.");
			}
		}
	}
}
