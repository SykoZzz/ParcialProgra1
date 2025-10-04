using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PortalAcademicoApp.Data;

var builder = WebApplication.CreateBuilder(args);

// -------------------
// Base de datos (SQLite)
// -------------------
var conn = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=portalacademico.db";
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(conn));

// -------------------
// Identity + Roles
// -------------------
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

// -------------------
// Redis (IDistributedCache)
// -------------------
// Se prioriza variable de entorno Render, sino se usa Redis Cloud directo
var redisConn = builder.Configuration["Redis__ConnectionString"] 
                ?? "redis-15597.c258.us-east-1-4.ec2.redns.redis-cloud.com:15597,password=RDP9OBeM26AjmwKvTBnZp3oVLYxx0Wy4,ssl=True,abortConnect=False";

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConn;
});

// -------------------
// Session (usa Redis si está configurado)
// -------------------
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// HttpContextAccessor (para _Layout y sesión)
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllersWithViews();

var app = builder.Build();

// -------------------
// Migraciones + seed
// -------------------
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var db = services.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
    await SeedData.EnsureSeedDataAsync(services);
}

// -------------------
// Middleware
// -------------------
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession(); // antes de auth
app.UseAuthentication();
app.UseAuthorization();

// -------------------
// Map routes
// -------------------
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);
app.MapRazorPages();

// -------------------
// Puerto dinámico Render
// -------------------
var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
app.Run($"http://0.0.0.0:{port}");
