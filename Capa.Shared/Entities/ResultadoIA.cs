using System.ComponentModel.DataAnnotations;

namespace Capa.Shared.Entities
{
    public class ResultadoIA
    {
        public int Id { get; set; }

        public string? ObservacionGeneral { get; set; }
        public DateTime Fecha { get; set; }

        public Estudiante Estudiante { get; set; } = null!;

        public int EstudianteId { get; set; }
        public ICollection<Recomendacion> Recomendaciones { get; set; } = [];
    }
}
