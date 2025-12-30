using Capa.Shared.DTOs;
using Capa.Shared.Entities;
using Capa.Shared.Responses;

namespace Capa.Backend.Repositories.Intefaces
{
    public interface IRespuestasRepository
    {
        Task<ActionResponse<bool>> AddAsync(RegistroRespuestasDTO respuestaDTO);

        Task<ActionResponse<IEnumerable<Respuesta>>> ConsultaAsync(int estudianteId);
        //Task<ActionResponse<IEnumerable<Respuesta>>> ConsultaAsync(int estudianteId);
    }
}
