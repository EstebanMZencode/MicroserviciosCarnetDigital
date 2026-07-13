using System.Net.Http.Headers;
using System.Text.Json;

namespace MicroServicioUsuarios.Services.ExternalServices
{
    public class TipoIdentificacionE_Service : ITipoIdentificacionE_Service
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        public enum TipoIdentificacionResponse
        {
            Success,             // Existe y estado true
            Inactive,            // Existe pero estado false
            NotFound,            // No existe
            ServiceUnavailable,  // Error de conexión
            Unauthorized         // No Autorizado
        }

        public TipoIdentificacionE_Service(
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<(TipoIdentificacionResponse, string[] error)> ValidarTipoIdentificacionAsync(string tipoIdentificacionID, string token)
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
                        return (TipoIdentificacionResponse.Unauthorized, 
                            new[] { "El servicio de tipos de identificación no autorizó la conexión." });
                    }

                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        return (TipoIdentificacionResponse.NotFound, 
                            new[] { "Tipo de identificación no encontrada." });
                    }
                    

                    return (TipoIdentificacionResponse.ServiceUnavailable, 
                        new[] { "Servicio de tipos de identificación no disponible. Intente más tarde." });
                }

                // Parsear json
                var json = await response.Content.ReadAsStringAsync();
                using var document = JsonDocument.Parse(json);

                // Lee estado del json. Si no existe o es false, devuelve false.
                if (!document.RootElement.TryGetProperty("estado", out var estadoProp))
                {
                    return (TipoIdentificacionResponse.ServiceUnavailable,
                        new[] { "Respuesta inválida del servicio de tipos de identificación." });
                }

                if (estadoProp.GetBoolean())
                {
                    return (TipoIdentificacionResponse.Success, Array.Empty<string>());
                }
                else
                {
                    return (TipoIdentificacionResponse.Inactive,
                        new[] { "Tipo de identificación inactivo." });
                }

            }
            catch
            {
                return (TipoIdentificacionResponse.ServiceUnavailable, 
                    new[] { "Servicio de tipos de identificación no disponible. Intente más tarde" });
            }
            
        }
    }
}