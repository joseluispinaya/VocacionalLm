using Capa.Backend.Data;
using Capa.Backend.Repositories.Intefaces;
using Capa.Shared.DTOs;
using Capa.Shared.Entities;
using Capa.Shared.Responses;
using Microsoft.EntityFrameworkCore;

namespace Capa.Backend.Repositories.Implementations
{
    public class RespuestasRepository : IRespuestasRepository
    {
        private readonly DataContext _context;
        public RespuestasRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<ActionResponse<bool>> AddAsync(RegistroRespuestasDTO respuestaDTO)
        {
            var estudiante = await _context.Estudiantes
                .FirstOrDefaultAsync(e => e.Id == respuestaDTO.EstudianteId);

            if (estudiante is null)
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = "Estudiante no encontrado."
                };
            }

            var preguntaIds = respuestaDTO.Respuestas
                .Select(r => r.PreguntaId)
                .ToList();

            var preguntasValidas = await _context.Preguntas
                .Where(p => preguntaIds.Contains(p.Id))
                .Select(p => p.Id)
                .ToListAsync();

            if (preguntasValidas.Count != preguntaIds.Count)
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = "Una o más preguntas no existen."
                };
            }

            var respuestas = respuestaDTO.Respuestas.Select(item => new Respuesta
            {
                EstudianteId = respuestaDTO.EstudianteId,
                PreguntaId = item.PreguntaId,
                TextoRespuesta = item.TextoRespuesta
            });

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                _context.Respuestas.AddRange(respuestas);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new ActionResponse<bool>
                {
                    WasSuccess = true,
                    Message = "Respuestas registradas correctamente.",
                    Result = true
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<ActionResponse<IEnumerable<Respuesta>>> ConsultaAsync(int estudianteId)
        {
            var estudianteExiste = await _context.Estudiantes.AnyAsync(e => e.Id == estudianteId);

            if (!estudianteExiste)
            {
                return new ActionResponse<IEnumerable<Respuesta>>
                {
                    WasSuccess = false,
                    Message = "El estudiante no existe."
                };
            }

            var respuestas = await _context.Respuestas
                .Where(r => r.EstudianteId == estudianteId)
                .Include(r => r.Pregunta)
                .ToListAsync();

            if (!respuestas.Any())
            {
                return new ActionResponse<IEnumerable<Respuesta>>
                {
                    WasSuccess = false,
                    Message = "El estudiante no tiene respuestas registradas."
                };
            }

            return new ActionResponse<IEnumerable<Respuesta>>
            {
                WasSuccess = true,
                Result = respuestas
            };
        }
    }
}
