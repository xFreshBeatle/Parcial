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

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

    await dbContext.Database.MigrateAsync();
    await EnsureSeedIdentityAsync(roleManager, userManager);
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
app.UseHttpsRedirection();
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

static async Task EnsureSeedIdentityAsync(RoleManager<IdentityRole> roleManager, UserManager<IdentityUser> userManager)
{
    const string analistaRole = "Analista";
    const string seedPassword = "Parcial2026!";

    var seedUsers = new[]
    {
        new { Email = "analista@parcial.com", IsAnalista = true },
        new { Email = "cliente1@parcial.com", IsAnalista = false },
        new { Email = "cliente2@parcial.com", IsAnalista = false }
    };

    if (!await roleManager.RoleExistsAsync(analistaRole))
    {
        var createRoleResult = await roleManager.CreateAsync(new IdentityRole(analistaRole));
        if (!createRoleResult.Succeeded)
        {
            throw new InvalidOperationException("No se pudo crear el rol Analista.");
        }
    }

    foreach (var seedUser in seedUsers)
    {
        var user = await userManager.FindByEmailAsync(seedUser.Email);
        if (user is null)
        {
            user = new IdentityUser
            {
                UserName = seedUser.Email,
                Email = seedUser.Email,
                EmailConfirmed = true
            };

            var createUserResult = await userManager.CreateAsync(user, seedPassword);
            if (!createUserResult.Succeeded)
            {
                throw new InvalidOperationException($"No se pudo crear el usuario semilla {seedUser.Email}.");
            }
        }
        else
        {
            user.UserName = seedUser.Email;
            user.Email = seedUser.Email;
            user.EmailConfirmed = true;

            var updateUserResult = await userManager.UpdateAsync(user);
            if (!updateUserResult.Succeeded)
            {
                throw new InvalidOperationException($"No se pudo actualizar el usuario semilla {seedUser.Email}.");
            }

            var resetToken = await userManager.GeneratePasswordResetTokenAsync(user);
            var resetPasswordResult = await userManager.ResetPasswordAsync(user, resetToken, seedPassword);
            if (!resetPasswordResult.Succeeded)
            {
                throw new InvalidOperationException($"No se pudo restablecer la contraseña del usuario {seedUser.Email}.");
            }
        }

        if (seedUser.IsAnalista)
        {
            if (!await userManager.IsInRoleAsync(user, analistaRole))
            {
                await userManager.AddToRoleAsync(user, analistaRole);
            }
        }
        else
        {
            if (await userManager.IsInRoleAsync(user, analistaRole))
            {
                await userManager.RemoveFromRoleAsync(user, analistaRole);
            }
        }
    }
}
