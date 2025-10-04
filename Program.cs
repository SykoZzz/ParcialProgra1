using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PortalAcademicoApp.Data;
using StackExchange.Redis;
using System.Security.Authentication;

var builder = WebApplication.CreateBuilder(args);

// -------------------
// Base de datos (SQLite)
// -------------------
var conn = builder.Configuration.GetConnectionString("DefaultConnection") 
           ?? "Data Source=portalacademico.db";
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
// Redis (IDistributedCache) - Development vs Production
// -------------------
if (builder.Environment.IsDevelopment())
{
    // Redis local para pruebas
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = "localhost:6379";
    });
}
else
{
    // Redis Cloud en producción
    var redisConn = builder.Configuration["Redis__ConnectionString"];
    if (!string.IsNullOrEmpty(redisConn))
    {
        var redisOptions = ConfigurationOptions.Parse(redisConn);
        redisOptions.AbortOnConnectFail = false;
        redisOptions.ConnectTimeout = 10000;
        redisOptions.SyncTimeout = 10000;
        redisOptions.KeepAlive = 60;
        redisOptions.ReconnectRetryPolicy = new ExponentialRetry(5000);
        redisOptions.Ssl = true;
        redisOptions.User = "default";            
        redisOptions.Password = "RDP9OBeM26AjmwKvTBnZp3oVLYxx0Wy4"; 
        redisOptions.SslProtocols = SslProtocols.Tls12;

        builder.Services.AddStackExchangeRedisCache(options =>
        {
            options.ConfigurationOptions = redisOptions;
        });
    }
    else
    {
        // fallback seguro
        builder.Services.AddDistributedMemoryCache();
    }
}

// -------------------
// Session (Redis o memoria)
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// -------------------
// HttpContextAccessor (para _Layout y sesión)
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllersWithViews();

var app = builder.Build();

// -------------------
// Migraciones + seed
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var db = services.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
    await SeedData.EnsureSeedDataAsync(services);
}

// -------------------
// Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

// -------------------
// Map routes
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Cursos}/{action=Index}/{id?}"
);
app.MapRazorPages();

// -------------------
// Puerto dinámico Render / localhost
var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
app.Run($"http://0.0.0.0:{port}");
