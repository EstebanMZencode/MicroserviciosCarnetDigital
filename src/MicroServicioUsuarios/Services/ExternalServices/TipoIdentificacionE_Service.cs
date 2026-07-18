using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace MicroServicioUsuarios.Services.ExternalServices
{

    public class TipoIdentificacionE_Service : ITipoIdentificacionE_Service
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        public TipoIdentificacionE_Service(
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<MicroServicesResponse> ValidarTipoIdentificacionIDAsync(string tipoIdentificacionID, string token)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("TipoIdentificacionClient");
                var urlBase = _configuration["MicroServicios:TipoIdentificacionUrl"];
                var url = $"{urlBase}/tiposidentificacion/{tipoIdentificacionID}";

                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

                var response = await client.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    return response.StatusCode switch
                    {
                        HttpStatusCode.Unauthorized => MicroServicesResponse.Unauthorized(
                            errors: new Dictionary<string, string[]>
                            {
                                { "TiposIdentificacionService", new[] { "El servicio de tipos de identificación no autorizó la conexión." } }
                            }),

                        HttpStatusCode.NotFound => MicroServicesResponse.NotFound(
                            errors: new Dictionary<string, string[]>
                            {
                                { "TiposIdentificacionService", new[] { "Tipo de identificación no encontrada." } }
                            }),

                        _ => MicroServicesResponse.ServiceUnavailable(
                            errors: new Dictionary<string, string[]>
                            {
                                { "TiposIdentificacionService", new[] { "Servicio de tipos de identificación no disponible. Intente más tarde." } }
                            })
                    };
                }

                var json = await response.Content.ReadAsStringAsync();
                var document = JsonDocument.Parse(json);

                if (!document.RootElement.TryGetProperty("estado", out var estadoProp))
                {
                    return MicroServicesResponse.InternalServerError(
                        errors: new Dictionary<string, string[]>
                        {
                            { "TiposIdentificacionService", new[] { "Respuesta inválida del servicio de tipos de identificación." } }
                        });
                }

                return estadoProp.GetBoolean()
                    ? MicroServicesResponse.Success()
                    : MicroServicesResponse.BadRequest(
                        errors: new Dictionary<string, string[]>
                        {
                            { "TiposIdentificacionService", new[] { "Tipo de identificación inactivo." } }
                        });
            }
            catch
            {
                return MicroServicesResponse.ServiceUnavailable(
                    errors: new Dictionary<string, string[]>
                    {
                        { "TiposIdentificacionService", new[] { "Servicio de tipos de identificación no disponible. Intente más tarde." } }
                    });
            }
        }
    }
}