using Capa.Backend.Helpers;
using Capa.Backend.Repositories.Intefaces;
using Capa.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Capa.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RespuestasController : ControllerBase
    {
        private readonly IRespuestasRepository _respuestaRepository;
        private readonly IIARecommendationService _iARecommendationService;
        public RespuestasController(IRespuestasRepository respuestasRepository, IIARecommendationService iARecommendationService)
        {
            _respuestaRepository = respuestasRepository;
            _iARecommendationService = iARecommendationService;
        }

        [HttpPost("registrar")]
        public async Task<IActionResult> PostAsync([FromBody] RegistroRespuestasDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var action = await _respuestaRepository.AddAsync(request);

            if (action.WasSuccess)
            {
                return Ok(action);
            }

            return BadRequest(action);
        }

        [HttpPost("Consulta")]
        public async Task<IActionResult> ConsultaAsync([FromBody] CosultaDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var docentesResponse = await _respuestaRepository.ConsultaAsync(request.EstudianteId);

            if (!docentesResponse.WasSuccess || docentesResponse.Result == null || !docentesResponse.Result.Any())
                return BadRequest("No se encontraron respuestas para el estudiante seleccionado.");

            var listaDocentes = docentesResponse.Result!;

            var recomendacion = await _iARecommendationService.GenerarRecomendacionAsync(listaDocentes.ToList());

            if (!recomendacion.WasSuccess)
                return BadRequest(recomendacion.Message);

            return Ok(recomendacion.Result);
        }

    }
}
