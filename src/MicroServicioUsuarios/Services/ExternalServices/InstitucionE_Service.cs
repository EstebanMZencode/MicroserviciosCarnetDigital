using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace MicroServicioUsuarios.Services.ExternalServices
{
    public class InstitucionE_Service : IInstitucionE_Service
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        public InstitucionE_Service(
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<MicroServicesResponse> ValidarInstitucionIDAsync(string institucionID, string token)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("InstitucionClient");
                var urlBase = _configuration["MicroServicios:InstitucionesUrl"];
                var url = $"{urlBase}/institucion/{institucionID}";

                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

                var response = await client.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    return response.StatusCode switch
                    {
                        HttpStatusCode.Unauthorized => 
                            MicroServicesResponse.Unauthorized(
                                errors: new Dictionary<string, string[]>
                                {
                                    { "InstitucionesService", new[] { "El servicio de instituciones no autorizó la conexión." } }
                                }),
                            

                        HttpStatusCode.NotFound => 
                        MicroServicesResponse.NotFound(
                            errors: new Dictionary<string, string[]>
                            {
                                { "InstitucionesService", new[] { "Institución no encontrada." } }
                            }),

                        _ => MicroServicesResponse.ServiceUnavailable(
                                errors: new Dictionary<string, string[]>
                                {
                                    { "InstitucionesService", new[] { "Servicio de instituciones no disponible. Intente más tarde." } }
                                })
                    };
                }

                var json = await response.Content.ReadAsStringAsync();
                var document = JsonDocument.Parse(json);

                if (!document.RootElement.TryGetProperty("estado", out var estadoProp) || 
                    (!document.RootElement.TryGetProperty("dominios", out var dominiosProp) ||
                    dominiosProp.ValueKind != JsonValueKind.Array))
                {
                    return MicroServicesResponse.InternalServerError(
                        errors: new Dictionary<string, string[]>
                        {
                            { "InstitucionesService", new[] { "Respuesta inválida del servicio de instituciones." } }
                        });
                }

                if (!estadoProp.GetBoolean())
                {
                    return MicroServicesResponse.BadRequest(
                        errors: new Dictionary<string, string[]>
                        {
                            { "InstitucionesService", new[] { "Respuesta inválida del servicio de instituciones." } }
                        });
                }

                var dominiosList = dominiosProp
                    .EnumerateArray()
                    .Select(s => s.GetString())
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .ToArray()!;

                return MicroServicesResponse.Success(dominiosList);

            }
            catch
            {
                return MicroServicesResponse.ServiceUnavailable(
                errors: new Dictionary<string, string[]>
                {
                    { "InstitucionesService", new[] { "Servicio de instituciones no disponible. Intente más tarde." } }
                });
            }
        }
    }
}
