using System;
using System.ComponentModel.DataAnnotations;

namespace PortalAcademicoApp.Models
{
    public class Curso
    {
        public int Id { get; set; }

        [Required, StringLength(20)]
        public string Codigo { get; set; }

        [Required, StringLength(200)]
        public string Nombre { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Créditos debe ser mayor que 0")]
        public int Creditos { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Cupo máximo debe ser mayor que 0")]
        public int CupoMaximo { get; set; }

        [Required]
        public TimeSpan HorarioInicio { get; set; }

        [Required]
        public TimeSpan HorarioFin { get; set; }

        public bool Activo { get; set; } = true;
    }
}