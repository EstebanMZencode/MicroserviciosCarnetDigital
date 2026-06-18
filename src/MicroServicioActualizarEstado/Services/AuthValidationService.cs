using System.Text.Json;

namespace MicroServicioActualizarEstadoUsuario.Services
{
    public class AuthValidationService : IAuthValidationService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AuthValidationService> _logger;

        public AuthValidationService(IHttpClientFactory httpFactory, IConfiguration config, ILogger<AuthValidationService> logger)
        {
            _httpClient = httpFactory.CreateClient();
            _httpClient.BaseAddress = new Uri(config["MicroServicios:AuthValidateUrl"]!);
            _logger = logger;
        }

        public async Task<bool> ValidarTokenAsync(string token)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, "");
                request.Headers.Add("Authorization", $"Bearer {token}");

                var response = await _httpClient.SendAsync(request);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validando token");
                return false;
            }
        }
    } 
}