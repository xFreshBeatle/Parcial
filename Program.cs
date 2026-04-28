using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Parcial.Data;

var builder = WebApplication.CreateBuilder(args);

var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(port))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.ConfigureApplicationCookie(options =>
{
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Ensure database directory exists and run migrations
try
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

        // Create directory if it doesn't exist
        var dbPath = dbContext.Database.GetConnectionString()?.Split("=")[1]?.Split(";")[0];
        if (!string.IsNullOrWhiteSpace(dbPath))
        {
            var dbDirectory = Path.GetDirectoryName(dbPath);
            if (!string.IsNullOrWhiteSpace(dbDirectory) && !Directory.Exists(dbDirectory))
            {
                Directory.CreateDirectory(dbDirectory);
            }
        }

        // Apply migrations with timeout
        var cts = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(30));
        await dbContext.Database.MigrateAsync(cts.Token);
        await EnsureSeedIdentityAsync(roleManager, userManager);
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error durante inicialización de BD: {ex.Message}");
    Console.WriteLine($"Stack trace: {ex.StackTrace}");
    // No falla la aplicación, continúa de todas formas
}

static async Task EnsureSeedIdentityAsync(RoleManager<IdentityRole> roleManager, UserManager<IdentityUser> userManager)
{
    const string analistaRole = "Analista";
    const string seedPassword = "Parcial2026!";

    // Crear rol Analista si no existe
    if (!await roleManager.RoleExistsAsync(analistaRole))
    {
        await roleManager.CreateAsync(new IdentityRole(analistaRole));
    }

    // Usuarios de prueba
    var seedUsers = new[]
    {
        new { Email = "analista@parcial.com", IsAnalista = true },
        new { Email = "cliente1@parcial.com", IsAnalista = false },
        new { Email = "cliente2@parcial.com", IsAnalista = false }
    };

    foreach (var seedUser in seedUsers)
    {
        var user = await userManager.FindByEmailAsync(seedUser.Email);
        if (user == null)
        {
            user = new IdentityUser
            {
                UserName = seedUser.Email,
                Email = seedUser.Email,
                EmailConfirmed = true
            };

            var createResult = await userManager.CreateAsync(user, seedPassword);
            if (!createResult.Succeeded)
            {
                throw new InvalidOperationException($"No se pudo crear usuario: {seedUser.Email}");
            }
        }

        // Asignar rol Analista si corresponde
        if (seedUser.IsAnalista)
        {
            if (!await userManager.IsInRoleAsync(user, analistaRole))
            {
                await userManager.AddToRoleAsync(user, analistaRole);
            }
        }
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

var forwardedHeadersOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
};
forwardedHeadersOptions.KnownIPNetworks.Clear();
forwardedHeadersOptions.KnownProxies.Clear();
app.UseForwardedHeaders(forwardedHeadersOptions);
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
   .WithStaticAssets();

app.Run();
