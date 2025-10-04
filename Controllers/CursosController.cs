using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using PortalAcademicoApp.Data;
using PortalAcademicoApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace PortalAcademicoApp.Controllers
{
    public class CursosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IDistributedCache _cache;
        private const string ActiveCoursesCacheKey = "ActiveCourses";

        public CursosController(ApplicationDbContext context, IDistributedCache cache)
        {
            _context = context;
            _cache = cache;
        }

        // GET: /Cursos
        public async Task<IActionResult> Index(string nombre, int? creditosMin, int? creditosMax)
        {
            // Si no hay filtros, usar cache (60s)
            if (string.IsNullOrWhiteSpace(nombre) && creditosMin == null && creditosMax == null)
            {
                var cached = await _cache.GetStringAsync(ActiveCoursesCacheKey);
                if (!string.IsNullOrEmpty(cached))
                {
                    var list = JsonSerializer.Deserialize<List<Curso>>(cached);
                    return View(list);
                }

                var listFromDb = await _context.Cursos.Where(c => c.Activo).ToListAsync();
                var json = JsonSerializer.Serialize(listFromDb);
                await _cache.SetStringAsync(ActiveCoursesCacheKey, json, new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60)
                });
                return View(listFromDb);
            }
            else
            {
                var query = _context.Cursos.AsQueryable().Where(c => c.Activo);
                if (!string.IsNullOrWhiteSpace(nombre))
                    query = query.Where(c => c.Nombre.Contains(nombre));
                if (creditosMin.HasValue)
                    query = query.Where(c => c.Creditos >= creditosMin.Value);
                if (creditosMax.HasValue)
                    query = query.Where(c => c.Creditos <= creditosMax.Value);

                var filtered = await query.ToListAsync();
                return View(filtered);
            }
        }

        // GET: /Cursos/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var curso = await _context.Cursos.FindAsync(id);
            if (curso == null) return NotFound();
            HttpContext.Session.SetString("LastVisitedCourseId", curso.Id.ToString());
            HttpContext.Session.SetString("LastVisitedCourseName", curso.Nombre);
            if (curso.Creditos <= 0) ModelState.AddModelError("", "Créditos inválidos (<=0).");
            if (curso.HorarioInicio >= curso.HorarioFin) ModelState.AddModelError("", "Horario Inicio debe ser anterior al Horario Fin.");

            return View(curso);
        }
    }
}