using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PortalAcademicoApp.Models;
using System;

namespace PortalAcademicoApp.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Curso> Cursos { get; set; }
        public DbSet<Matricula> Matriculas { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Curso>()
                   .HasIndex(c => c.Codigo)
                   .IsUnique();

            builder.Entity<Matricula>()
                   .HasIndex(m => new { m.CursoId, m.UsuarioId })
                   .IsUnique();

            // TimeSpan -> string converter para SQLite ("HH:mm")
            var timeSpanConverter = new ValueConverter<TimeSpan, string>(
                v => v.ToString(@"hh\:mm"),
                v => TimeSpan.Parse(v));

            builder.Entity<Curso>().Property(c => c.HorarioInicio).HasConversion(timeSpanConverter);
            builder.Entity<Curso>().Property(c => c.HorarioFin).HasConversion(timeSpanConverter);
        }
    }
}