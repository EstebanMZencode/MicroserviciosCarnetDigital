using System.Text;
using System.Text.Json;
using BCrypt.Net;
using MicroServicioUsuarios.Entities;
using MicroServicioUsuarios.Repository;
using Microsoft.Data.SqlClient;

namespace MicroServicioUsuarios.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<UsuarioService> _logger;

        public UsuarioService(
            IUsuarioRepository usuarioRepository,
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory,
            ILogger<UsuarioService> logger)
        {
            _usuarioRepository = usuarioRepository;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<(UsuarioCreateResponse? data, int statusCode, string? error)> CrearUsuarioAsync(
            UsuarioCreateRequest request, string token)
        {
            // --- Validar token ---
            if (!await ValidarTokenAsync(token))
                return (null, 401, "No autorizado.");

            // --- VALIDAR REGLAS DE NEGOCIO BÁSICAS ---
            foreach (var inst in request.Instituciones)
            {
                // Validaciones que tuve que borrar
            }

            // -- encriptar contraseña
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            // --- Crear usuario ---
            Guid usuarioID;
            List<string> emailsCreados;

            try
            {
                _logger.LogInformation("Iniciando creación de usuario: {Identificacion}", request.Identificacion);
                (usuarioID, emailsCreados) = await _usuarioRepository.CrearUsuarioCompletoAsync(request, passwordHash);
                _logger.LogInformation("Usuario creado exitosamente: {UsuarioID}", usuarioID);
            }
            catch (SqlException ex) when (ex.Number >= 50010 && ex.Number <= 50072)
            {
                _logger.LogWarning(ex, "Error de negocio del SP: {Message}", ex.Message);
                return (null, 400, ex.Message);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Error SQL {Number}: {Message}", ex.Number, ex.Message);
                return (null, 500, $"Error de base de datos: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado: {Message}", ex.Message);
                return (null, 500, $"Error inesperado: {ex.Message}");
            }

            // --- construit response ---
            var response = new UsuarioCreateResponse
            {
                UsuarioID = usuarioID,
                TipoIdentificacion = request.TipoIdentID.ToString(),
                Identificacion = request.Identificacion,
                NombreCompleto = request.NombreCompleto,
                NombreEstado = request.EstadoID.ToString(), // Temporal, idealmente resolver nombre
                Emails = request.Emails.Select(e => new EmailResponse
                {
                    Email = e.Email,
                    Institucion = e.InstitucionID.ToString() // Temporal
                }).ToList(),
                Telefonos = request.Telefonos,
                Instituciones = request.Instituciones.Select(i => new InstitucionResponse
                {
                    Institucion = i.InstitucionID.ToString(),
                    TipoUsuario = i.TipoUsuarioID.ToString(),
                    Rol = i.RolID.ToString(),
                    FechaVencimientoCarnet = i.FechaVencimientoCarnet,
                    Carreras = i.Carreras.Select(c => c.ToString()).ToList(),
                    Areas = i.Areas.Select(a => a.ToString()).ToList()
                }).ToList()
            };

            // --- Bitacora
            foreach (var email in emailsCreados)
            {
                _ = RegistrarBitacoraAsync(token, email,
                    $"Creación de usuario {request.Identificacion}: {request.NombreCompleto}");
            }

            return (response, 201, null);
        }

        // --- Métodos privados ---
        private async Task<bool> ValidarTokenAsync(string token)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var request = new HttpRequestMessage(HttpMethod.Get, _configuration["MicroServicios:AuthValidateUrl"]!);
                request.Headers.Add("token", token);
                var response = await client.SendAsync(request);
                if (!response.IsSuccessStatusCode) return false;
                return (await response.Content.ReadAsStringAsync()).Contains("true", StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error validando token.");
                return false;
            }
        }

        private async Task RegistrarBitacoraAsync(string token, string email, string descripcion)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var body = JsonSerializer.Serialize(new { usuarioID = email, descripcion });
                var request = new HttpRequestMessage(HttpMethod.Post, _configuration["MicroServicios:BitacoraUrl"]!)
                {
                    Content = new StringContent(body, Encoding.UTF8, "application/json")
                };
                request.Headers.Add("token", token);
                await client.SendAsync(request);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error al registrar bitácora para {Email}.", email);
            }
        }
    }
}