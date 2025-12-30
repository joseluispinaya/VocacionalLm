using Capa.Shared.DTOs;
using Capa.Shared.Entities;
using Capa.Shared.Responses;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Capa.Backend.Helpers
{
    public class IARecommendationService : IIARecommendationService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _http;

        public IARecommendationService(HttpClient http, IConfiguration configuration)
        {
            _http = http;
            _configuration = configuration;
        }

        public async Task<ActionResponse<List<RecomendadoDTO>>> GenerarRecomendacionAsync(List<Respuesta> respuestas)
        {
            var apiKey = _configuration["OpenAIKey"];

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                return new ActionResponse<List<RecomendadoDTO>>
                {
                    WasSuccess = false,
                    Message = "OpenAI API Key no configurada."
                };
            }

            var prompt = ConstruirPrompt(respuestas);

            var requestBody = new
            {
                model = "gpt-4.1-mini",
                messages = new[]
                {
                new { role = "system", content = "Eres un orientador vocacional profesional." },
                new { role = "user", content = prompt }
            },
                temperature = 0.3
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            request.Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            using var response = await _http.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                return new ActionResponse<List<RecomendadoDTO>>
                {
                    WasSuccess = false,
                    Message = "Error al comunicarse con OpenAI."
                };
            }

            var responseJson = await response.Content.ReadAsStringAsync();

            try
            {
                var resultado = ExtraerResultado(responseJson);

                return new ActionResponse<List<RecomendadoDTO>>
                {
                    WasSuccess = true,
                    Result = resultado
                };
            }
            catch
            {
                return new ActionResponse<List<RecomendadoDTO>>
                {
                    WasSuccess = false,
                    Message = "La respuesta de IA no tiene el formato esperado."
                };
            }
        }

        private List<RecomendadoDTO> ExtraerResultado(string json)
        {
            using var doc = JsonDocument.Parse(json);

            var content = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            return JsonSerializer.Deserialize<List<RecomendadoDTO>>(content!)!;
        }

        private string ConstruirPrompt(List<Respuesta> respuestas)
        {
            var sb = new StringBuilder();

            sb.AppendLine("Con base en las siguientes respuestas, recomienda 3 carreras:");
            sb.AppendLine();

            foreach (var d in respuestas)
            {
                sb.AppendLine($"Pregunta: {d.Pregunta.Texto}");
                sb.AppendLine($"Respuesta: {d.TextoRespuesta}");
                sb.AppendLine();
            }

            sb.AppendLine("Selecciona únicamente carreras apropiadas para el perfil.");
            sb.AppendLine("Devuelve máximo 3 resultados.");
            sb.AppendLine("Asigna una Puntuacion de 0 a 100.");
            sb.AppendLine("Responde exclusivamente con un JSON válido.");
            sb.AppendLine("Formato:");
            sb.AppendLine("[{\"Carrera\":\"\",\"Puntuacion\":0,\"Justificacion\":\"\"}]");

            return sb.ToString();
        }
    }
}
