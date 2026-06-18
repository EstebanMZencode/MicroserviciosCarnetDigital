using System.Text;
using System.Text.Json;

namespace MicroServicioActualizarEstadoUsuario.Services
{
    public class BitacoraService : IBitacoraService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<BitacoraService> _logger;

        public BitacoraService(IHttpClientFactory httpFactory, IConfiguration config, ILogger<BitacoraService> logger)
        {
            _httpClient = httpFactory.CreateClient();
            _httpClient.BaseAddress = new Uri(config["MicroServicios:BitacoraUrl"]!);
            _logger = logger;
        }

        public async Task RegistrarAsync(string usuario, string accion, string? detalle = null)
        {
            try
            {
                var payload = new
                {
                    Usuario = usuario,
                    Descripcion = accion,
                    Detalle = detalle,
                    Fecha = DateTime.UtcNow
                };

                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Intentar registrar en bitácora (fire-and-forget con log si falla)
                var response = await _httpClient.PostAsync("", content);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Bitácora respondió {StatusCode} para usuario {Usuario}",
                        response.StatusCode, usuario);
                }
            }
            catch (Exception ex)
            {
                // No debe fallar el microservicio principal si la bitácora falla
                _logger.LogError(ex, "Error al registrar bitácora para usuario {Usuario}", usuario);
            }
        }
    }
}