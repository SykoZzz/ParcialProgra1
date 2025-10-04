using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using PortalAcademicoApp.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PortalAcademicoApp.Data
{
    public static class SeedData
    {
        public static async Task EnsureSeedDataAsync(IServiceProvider serviceProvider)
        {
            var userMgr = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var roleMgr = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var db = serviceProvider.GetRequiredService<ApplicationDbContext>();

            var roleName = "Coordinador";
            if (!await roleMgr.RoleExistsAsync(roleName))
            {
                await roleMgr.CreateAsync(new IdentityRole(roleName));
            }

            var coordEmail = "coordinador@uni.edu";
            var coord = await userMgr.FindByEmailAsync(coordEmail);
            if (coord == null)
            {
                coord = new IdentityUser { UserName = coordEmail, Email = coordEmail, EmailConfirmed = true };
                await userMgr.CreateAsync(coord, "P@ssw0rd123!");
                await userMgr.AddToRoleAsync(coord, roleName);
            }

            if (!db.Cursos.Any())
            {
                db.Cursos.AddRange(
                    new Curso { Codigo="PROG101", Nombre="Programación I", Creditos=3, CupoMaximo=30, HorarioInicio=TimeSpan.Parse("08:00"), HorarioFin=TimeSpan.Parse("10:00"), Activo=true },
                    new Curso { Codigo="MAT101", Nombre="Matemáticas I", Creditos=4, CupoMaximo=25, HorarioInicio=TimeSpan.Parse("10:00"), HorarioFin=TimeSpan.Parse("12:00"), Activo=true },
                    new Curso { Codigo="FIS101", Nombre="Física I", Creditos=3, CupoMaximo=20, HorarioInicio=TimeSpan.Parse("14:00"), HorarioFin=TimeSpan.Parse("16:00"), Activo=true }
                );
                await db.SaveChangesAsync();
            }
        }
    }
}