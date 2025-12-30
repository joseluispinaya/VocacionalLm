using System.ComponentModel.DataAnnotations;

namespace Capa.Shared.DTOs
{
    public class CosultaDTO
    {
        [Range(1, int.MaxValue, ErrorMessage = "Debes seleccionar una Estudiante.")]
        public int EstudianteId { get; set; }
    }
}
