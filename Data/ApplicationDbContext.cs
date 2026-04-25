using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Parcial.Models;

namespace Parcial.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<IdentityUser>(options)
{
	private const string AnalistaRoleId = "4f6fd1d9-b1ef-4200-9827-6ce304f2b001";
	private const string AnalistaUserId = "c31ccfbc-df70-456b-9666-4f083ec3f08e";
	private const string ClienteUnoUserId = "f489a6ff-7eb1-4d5a-86d8-5bf910ca0701";
	private const string ClienteDosUserId = "1a311f98-fd47-47d4-9a11-4fbd56f8de03";
	private const string SeedPasswordHash = "AQAAAAIAAYagAAAAEIJrgXtc5St3z4Z5kh9XpgeSk6nQvrj6yqNPT0R7LIa/73TLwZNLFAbojebTtOLung==";

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
					UsuarioId = ClienteUnoUserId,
					IngresosMensuales = 1000m,
					Activo = true
				},
				new Cliente
				{
					Id = 2,
					UsuarioId = ClienteDosUserId,
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

		var analistaUser = new IdentityUser
		{
			Id = AnalistaUserId,
			UserName = "analista@parcial.com",
			NormalizedUserName = "ANALISTA@PARCIAL.COM",
			Email = "analista@parcial.com",
			NormalizedEmail = "ANALISTA@PARCIAL.COM",
			EmailConfirmed = true,
			SecurityStamp = "EAAE2E2A-A58F-4B08-B9EE-E06D9EA82185",
			ConcurrencyStamp = "6D71E2FC-A9BD-4186-81D0-6AC7A9E85683"
		};

		analistaUser.PasswordHash = SeedPasswordHash;

		var clienteUnoUser = new IdentityUser
		{
			Id = ClienteUnoUserId,
			UserName = "cliente1@parcial.com",
			NormalizedUserName = "CLIENTE1@PARCIAL.COM",
			Email = "cliente1@parcial.com",
			NormalizedEmail = "CLIENTE1@PARCIAL.COM",
			EmailConfirmed = true,
			SecurityStamp = "7A31D7D1-5B16-4A78-80FA-5AF8E3310C5D",
			ConcurrencyStamp = "D8689711-5F43-40B6-8F06-71DBDECC80B7",
			PasswordHash = SeedPasswordHash
		};

		var clienteDosUser = new IdentityUser
		{
			Id = ClienteDosUserId,
			UserName = "cliente2@parcial.com",
			NormalizedUserName = "CLIENTE2@PARCIAL.COM",
			Email = "cliente2@parcial.com",
			NormalizedEmail = "CLIENTE2@PARCIAL.COM",
			EmailConfirmed = true,
			SecurityStamp = "B6B4B3CA-F8A9-430E-9C9D-B61CC688D272",
			ConcurrencyStamp = "75925CD1-08AF-49A1-98C1-4E56E8A6AE04",
			PasswordHash = SeedPasswordHash
		};

		builder.Entity<IdentityRole>().HasData(role);
		builder.Entity<IdentityUser>().HasData(analistaUser, clienteUnoUser, clienteDosUser);
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
