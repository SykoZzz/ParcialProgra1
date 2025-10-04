using System;
using System.ComponentModel.DataAnnotations;

namespace PortalAcademicoApp.Models
{
    public enum EstadoMatricula { Pendiente = 0, Confirmada = 1, Cancelada = 2 }

    public class Matricula
    {
        public int Id { get; set; }

        [Required]
        public int CursoId { get; set; }
        public Curso Curso { get; set; }

        [Required]
        public string UsuarioId { get; set; }

        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

        public EstadoMatricula Estado { get; set; } = EstadoMatricula.Pendiente;
    }
}