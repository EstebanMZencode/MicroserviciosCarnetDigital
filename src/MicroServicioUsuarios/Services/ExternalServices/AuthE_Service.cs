using System.Net.Http.Headers;

namespace MicroServicioUsuarios.Services.ExternalServices
{
    public class AuthE_Service : IAuthE_Service
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        public AuthE_Service(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<bool> ValidarTokenAsync(string token)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("AuthClient");
                var urlBase = _configuration["MicroServicios:AuthUrl"];
                var url = $"{urlBase}/api/validate";

                client.DefaultRequestHeaders.Add("token", token);

                var response = await client.GetAsync(url);

                // Si la respuesta es exitosa y el contenido es "true", entonces el token es válido
                return response.IsSuccessStatusCode && 
                    (await response.Content.ReadAsStringAsync()).Trim() == "\"true\"";
            }
            catch (Exception)
            {
                return false;
            }
            
        }
    }
}
