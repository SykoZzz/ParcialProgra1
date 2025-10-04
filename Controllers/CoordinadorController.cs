using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using PortalAcademicoApp.Data;
using PortalAcademicoApp.Models;
using System.Linq;
using System.Threading.Tasks;

namespace PortalAcademicoApp.Controllers
{
    [Authorize(Roles = "Coordinador")]
    public class CoordinadorController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IDistributedCache _cache;
        private const string ActiveCoursesCacheKey = "ActiveCourses";

        public CoordinadorController(ApplicationDbContext context, IDistributedCache cache)
        {
            _context = context;
            _cache = cache;
        }

        // Listar cursos
        public async Task<IActionResult> Cursos()
        {
            var cursos = await _context.Cursos.ToListAsync();
            return View(cursos);
        }

        // Create
        public IActionResult Create() => View();

        [HttpPost]
        public async Task<IActionResult> Create(Curso curso)
        {
            if (curso.HorarioInicio >= curso.HorarioFin)
            {
                ModelState.AddModelError("", "Horario inicio debe ser anterior al fin.");
            }
            if (!ModelState.IsValid) return View(curso);

            _context.Cursos.Add(curso);
            await _context.SaveChangesAsync();
            await _cache.RemoveAsync(ActiveCoursesCacheKey);
            TempData["Success"] = "Curso creado.";
            return RedirectToAction(nameof(Cursos));
        }

        // Edit
        public async Task<IActionResult> Edit(int id)
        {
            var curso = await _context.Cursos.FindAsync(id);
            if (curso == null) return NotFound();
            return View(curso);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, Curso curso)
        {
            if (id != curso.Id) return BadRequest();
            if (curso.HorarioInicio >= curso.HorarioFin)
            {
                ModelState.AddModelError("", "Horario inicio debe ser anterior al fin.");
            }
            if (!ModelState.IsValid) return View(curso);

            _context.Cursos.Update(curso);
            await _context.SaveChangesAsync();
            await _cache.RemoveAsync(ActiveCoursesCacheKey);
            TempData["Success"] = "Curso actualizado.";
            return RedirectToAction(nameof(Cursos));
        }

        [HttpPost]
        public async Task<IActionResult> Desactivar(int id)
        {
            var curso = await _context.Cursos.FindAsync(id);
            if (curso == null) return NotFound();
            curso.Activo = false;
            await _context.SaveChangesAsync();
            await _cache.RemoveAsync(ActiveCoursesCacheKey);
            TempData["Success"] = "Curso desactivado.";
            return RedirectToAction(nameof(Cursos));
        }

        // Matriculas por curso
        public async Task<IActionResult> Matriculas(int cursoId)
        {
            var lista = await _context.Matriculas
                .Include(m => m.Curso)
                .Where(m => m.CursoId == cursoId)
                .ToListAsync();
            ViewBag.Curso = await _context.Cursos.FindAsync(cursoId);
            return View(lista);
        }

        [HttpPost]
        public async Task<IActionResult> Confirmar(int id)
        {
            var m = await _context.Matriculas.Include(x => x.Curso).FirstOrDefaultAsync(x => x.Id == id);
            if (m == null) return NotFound();
            m.Estado = EstadoMatricula.Confirmada;
            await _context.SaveChangesAsync();
            TempData["Success"] = "Matrícula confirmada.";
            return RedirectToAction(nameof(Matriculas), new { cursoId = m.CursoId });
        }

        [HttpPost]
        public async Task<IActionResult> Cancelar(int id)
        {
            var m = await _context.Matriculas.Include(x => x.Curso).FirstOrDefaultAsync(x => x.Id == id);
            if (m == null) return NotFound();
            m.Estado = EstadoMatricula.Cancelada;
            await _context.SaveChangesAsync();
            TempData["Success"] = "Matrícula cancelada.";
            return RedirectToAction(nameof(Matriculas), new { cursoId = m.CursoId });
        }
    }
}
