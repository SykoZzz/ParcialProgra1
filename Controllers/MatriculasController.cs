using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortalAcademicoApp.Data;
using PortalAcademicoApp.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PortalAcademicoApp.Controllers
{
    public class MatriculasController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public MatriculasController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Inscribirse(int cursoId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var userId = user.Id;

            // 1. chequeo rapido duplicado
            if (await _context.Matriculas.AnyAsync(m => m.CursoId == cursoId && m.UsuarioId == userId && m.Estado != EstadoMatricula.Cancelada))
            {
                TempData["Error"] = "Ya estás matriculado en este curso.";
                return RedirectToAction("Details", "Cursos", new { id = cursoId });
            }

            using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                var curso = await _context.Cursos.FindAsync(cursoId);
                if (curso == null || !curso.Activo)
                {
                    TempData["Error"] = "Curso no disponible.";
                    return RedirectToAction("Index", "Cursos");
                }

                var inscritos = await _context.Matriculas.CountAsync(m => m.CursoId == cursoId && m.Estado != EstadoMatricula.Cancelada);
                if (inscritos >= curso.CupoMaximo)
                {
                    TempData["Error"] = "Cupo máximo alcanzado.";
                    return RedirectToAction("Details", "Cursos", new { id = cursoId });
                }

                // comprobar solapamiento
                var misMatriculas = await _context.Matriculas
                    .Include(m => m.Curso)
                    .Where(m => m.UsuarioId == userId && m.Estado != EstadoMatricula.Cancelada)
                    .ToListAsync();

                foreach (var m in misMatriculas)
                {
                    if (m.Curso.HorarioInicio < curso.HorarioFin && curso.HorarioInicio < m.Curso.HorarioFin)
                    {
                        TempData["Error"] = $"Solapamiento con {m.Curso.Nombre} ({m.Curso.HorarioInicio:hh\\:mm}-{m.Curso.HorarioFin:hh\\:mm}).";
                        return RedirectToAction("Details", "Cursos", new { id = cursoId });
                    }
                }

                // crear matricula
                var matricula = new Matricula
                {
                    CursoId = cursoId,
                    UsuarioId = userId,
                    Estado = EstadoMatricula.Pendiente,
                    FechaRegistro = DateTime.UtcNow
                };
                _context.Matriculas.Add(matricula);
                await _context.SaveChangesAsync();

                await tx.CommitAsync();
                TempData["Success"] = "Inscripción creada en estado Pendiente.";
                return RedirectToAction("Details", "Cursos", new { id = cursoId });
            }
            catch (DbUpdateException)
            {
                await tx.RollbackAsync();
                TempData["Error"] = "No fue posible inscribirse (error de concurrencia o duplicado).";
                return RedirectToAction("Details", "Cursos", new { id = cursoId });
            }
        }
    }
}