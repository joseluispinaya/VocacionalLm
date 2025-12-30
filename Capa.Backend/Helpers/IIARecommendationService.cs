using Capa.Shared.DTOs;
using Capa.Shared.Entities;
using Capa.Shared.Responses;

namespace Capa.Backend.Helpers
{
    public interface IIARecommendationService
    {
        Task<ActionResponse<List<RecomendadoDTO>>> GenerarRecomendacionAsync(List<Respuesta> respuestas);
    }
}
