using System.Text;
using System.Text.Json;
using MicroServicioActualizarEstadoUsuario.Entities;
using MicroServicioActualizarEstadoUsuario.Repository;
using Microsoft.Data.SqlClient;

namespace MicroServicioActualizarEstadoUsuario.Services
{
    public class EstadoUsuarioService : IEstadoUsuarioService
    {
        private readonly IEstadoUsuarioRepository _estadoUsuarioRepository;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<EstadoUsuarioService> _logger;

        public EstadoUsuarioService(
            IEstadoUsuarioRepository estadoUsuarioRepository,
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory,
            ILogger<EstadoUsuarioService> logger)
        {
            _estadoUsuarioRepository = estadoUsuarioRepository;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<(EstadoUsuarioResponse? data, int statusCode, string? error)>
            UpdateEstadoUsuarioAsync(string emailUsuario, Guid estadoId, string token)
        {
            if (!await ValidarTokenAsync(token))
                return (null, 401, "No autorizado.");

            EstadoUsuarioResponse? resultado;
            try
            {
                resultado = await _estadoUsuarioRepository.UpdateEstadoUsuarioAsync(emailUsuario, estadoId);
            }
            catch (SqlException ex) when (ex.Number is 50080 or 50081 or 50082)
            {
                return (null, 404, ex.Message);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Error de base de datos al actualizar estado para {Email}", emailUsuario);
                return (null, 500, "Error de base de datos. Inténtelo más tarde.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al actualizar estado para {Email}", emailUsuario);
                return (null, 500, "Error inesperado. Inténtelo más tarde.");
            }

            if (resultado is null)
                return (null, 404, "No se pudo actualizar el estado.");

            // Bitácora: UsuarioID como Guid (fire-and-forget)
            _ = RegistrarBitacoraAsync(token, resultado.UsuarioID,
                $"Cambio de estado usuario {emailUsuario}: Nuevo estado={resultado.NombreEstado}");

            return (resultado, 200, null);
        }

        // --- Métodos privados ------------------------------------------------------------
        private async Task<bool> ValidarTokenAsync(string token)
        {
            try
            {
                var validateUrl = _configuration["MicroServicios:AuthValidateUrl"]!;
                var client = _httpClientFactory.CreateClient();

                var request = new HttpRequestMessage(HttpMethod.Get, validateUrl);
                request.Headers.Add("token", token);

                var response = await client.SendAsync(request);
                if (!response.IsSuccessStatusCode) return false;

                var body = await response.Content.ReadAsStringAsync();
                return body.Contains("true", StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudo contactar al servicio de validación de token.");
                return false;
            }
        }

        // Registra en MicroServicioBitacoras (fire-and-forget).
        private async Task RegistrarBitacoraAsync(string token, Guid usuarioID, string descripcion)
        {
            try
            {
                var bitacoraUrl = _configuration["MicroServicios:BitacoraUrl"];
                var client = _httpClientFactory.CreateClient();

                var body = JsonSerializer.Serialize(new { usuarioID = usuarioID, descripcion });

                var request = new HttpRequestMessage(HttpMethod.Post, bitacoraUrl)
                {
                    Content = new StringContent(body, Encoding.UTF8, "application/json")
                };

                request.Headers.Add("token", token);

                var response = await client.SendAsync(request);
                
                _logger.LogInformation("Bitacora status: {Status}, body: {Body}",
                    response.StatusCode, await response.Content.ReadAsStringAsync()); 
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error al registrar en bitácora para el usuario '{UsuarioID}'.", usuarioID);
            }
        }
    }
}