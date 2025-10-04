using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PortalAcademicoApp.Data;

var builder = WebApplication.CreateBuilder(args);

// DB - SQLite por defecto (development)
var conn = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=portalacademico.db";
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(conn));

// Identity + Roles
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

// Redis config will be added in feature/sesion-redis (for now we still register session in that branch)

// HttpContextAccessor for layout session usage
builder.Services.AddHttpContextAccessor();

builder.Services.AddControllersWithViews();
var app = builder.Build();

// Aplicar migraciones y seed
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var db = services.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
    await SeedData.EnsureSeedDataAsync(services);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();