using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PortalAcademicoApp.Data;
using StackExchange.Redis;

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
// Redis (IDistributedCache) - configuración robusta
// -------------------
var redisConn = builder.Configuration["Redis__ConnectionString"] 
                ?? "redis-15597.c258.us-east-1-4.ec2.redns.redis-cloud.com:15597,password=RDP9OBeM26AjmwKvTBnZp3oVLYxx0Wy4,ssl=True,abortConnect=False";

var redisOptions = ConfigurationOptions.Parse(redisConn);
redisOptions.AbortOnConnectFail = false;                       // No falla si Redis tarda en conectar
redisOptions.ConnectTimeout = 10000;                            // 10s
redisOptions.SyncTimeout = 10000;                               // 10s
redisOptions.KeepAlive = 60;                                    // Mantener viva la conexión
redisOptions.ReconnectRetryPolicy = new ExponentialRetry(5000); // Reintento automático
redisOptions.SslProtocols = System.Security.Authentication.SslProtocols.Tls12; // TLS 1.2 obligatorio

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.ConfigurationOptions = redisOptions;
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
