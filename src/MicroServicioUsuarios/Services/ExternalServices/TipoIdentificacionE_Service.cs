using System.Net.Http.Headers;
using System.Text.Json;

namespace MicroServicioUsuarios.Services.ExternalServices
{
    public class TipoIdentificacionE_Service : ITipoIdentificacionE_Service
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ServicesStatus _serviceStatus;

        public TipoIdentificacionE_Service(
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory,
            ServicesStatus serviceStatus)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _serviceStatus = serviceStatus;
        }

        public async Task<(int statusCode, ServicesStatus.ServiceStatus, string message, string[] errors)> ValidarTipoIdentificacionAsync(string tipoIdentificacionID, string token)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("TipoIdentificacionClient");
                var urlBase = _configuration["MicroServicios:TipoIdentificacionUrl"];
                var url = $"{urlBase}/tiposidentificacion/{tipoIdentificacionID}";

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await client.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        return (401, _serviceStatus.Unauthorized, "Unauthorized",
                            new[] { "El servicio de tipos de identificación no autorizó la conexión." });
                    }

                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        return (404, _serviceStatus.NotFound, "Not Found",
                            new[] { "Tipo de identificación no encontrada." });
                    }
                    
                    return (503, _serviceStatus.ServiceUnavailable, "Service Unavailable",
                        new[] { "Servicio de tipos de identificación no disponible. Intente más tarde." });
                }

                // Parsear json
                var json = await response.Content.ReadAsStringAsync();
                using var document = JsonDocument.Parse(json);

                // Lee estado del json. Si no existe o es false, devuelve false.
                if (!document.RootElement.TryGetProperty("estado", out var estadoProp))
                {
                    return (503, _serviceStatus.ServiceUnavailable, "Service Unavailable",
                        new[] { "Respuesta inválida del servicio de tipos de identificación." });
                }

                if (estadoProp.GetBoolean())
                {
                    return (201, _serviceStatus.Success, string.Empty, Array.Empty<string>());
                }
                else
                {
                    return (400, _serviceStatus.Inactive, "Inactive",
                        new[] { "Tipo de identificación inactivo." });
                }

            }
            catch 
            {
                return (503, _serviceStatus.ServiceUnavailable, "Service Unavailable",
                    new[] { "Servicio de tipos de identificación no disponible. Intente más tarde" });
            }
            
        }
    }
}