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

        // Opciones de deserialización compartidas: insensible a mayúsculas
        // para tolerar las distintas convenciones de los microservicios externos.
        private static readonly JsonSerializerOptions _jsonOpts =
            new() { PropertyNameCaseInsensitive = true };

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

        // ── Creación de usuario (implementación existente, sin cambios) ───────────────

        public async Task<(UsuarioCreateResponse? data, int statusCode, string? error)> CrearUsuarioAsync(
            UsuarioCreateRequest request, string token)
        {
            if (!await ValidarTokenAsync(token))
                return (null, 401, "No autorizado.");

            foreach (var inst in request.Instituciones)
            {
                // Validaciones que tuve que borrar
            }

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

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

            var response = new UsuarioCreateResponse
            {
                UsuarioID = usuarioID,
                TipoIdentificacion = request.TipoIdentID.ToString(),
                Identificacion = request.Identificacion,
                NombreCompleto = request.NombreCompleto,
                NombreEstado = request.EstadoID.ToString(),
                Emails = request.Emails.Select(e => new EmailResponse
                {
                    Email = e.Email,
                    Institucion = e.InstitucionID.ToString()
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

            foreach (var email in emailsCreados)
                _ = RegistrarBitacoraAsync(token, email,
                    $"Creación de usuario {request.Identificacion}: {request.NombreCompleto}");

            return (response, 201, null);
        }

        // ── Detalle de usuario por email ──────────────────────────────────────────────

        public async Task<(UsuarioDetalleResponse? data, int statusCode, string? error)> GetDetalleByEmailAsync(
            string email, string token)
        {
            // Validar el JWT antes de tocar la BD
            if (!await ValidarTokenAsync(token))
                return (null, 401, "No autorizado.");

            // Consultar datos locales: Identificacion, NombreCompleto, TipoUsuario y los IDs
            UsuarioDetalleDB? dbData;
            try
            {
                dbData = await _usuarioRepository.GetDetalleByEmailAsync(email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error consultando datos de {Email}", email);
                return (null, 500, "Error al consultar los datos del usuario.");
            }

            if (dbData is null)
                return (null, 404, $"No se encontró un usuario con el email '{email}'.");

            // Resolver nombres de carreras y áreas en paralelo llamando a los
            // microservicios externos. Cada ID requiere una llamada individual.
            var nombresCarreras = await ResolverNombresCarrerasAsync(dbData.CarreraIDs, token);
            var nombresAreas = await ResolverNombresAreasAsync(dbData.AreaIDs, token);

            var response = new UsuarioDetalleResponse
            {
                Identificacion = dbData.Identificacion,
                NombreCompleto = dbData.NombreCompleto,
                TipoUsuario = dbData.NombreTipoUsuario,
                Carreras = nombresCarreras,
                Areas = nombresAreas
            };

            return (response, 200, null);
        }

        // ── Métodos privados ──────────────────────────────────────────────────────────

        // Llama a GET /api/carrera/{id} por cada ID y recoge el nombreCarrera.
        // Si una llamada falla se registra el error pero no se interrumpe el flujo:
        // el nombre queda omitido de la lista para no bloquear la respuesta completa.
        private async Task<List<string>> ResolverNombresCarrerasAsync(List<Guid> ids, string token)
        {
            if (ids.Count == 0) return new();

            var baseUrl = _configuration["MicroServicios:CarrerasUrl"]!.TrimEnd('/');
            var client = _httpClientFactory.CreateClient();
            var nombres = new List<string>();

            // Las llamadas se ejecutan en paralelo para no serializar la latencia de red
            var tareas = ids.Select(async id =>
            {
                try
                {
                    var request = new HttpRequestMessage(
                        HttpMethod.Get, $"{baseUrl}/api/carrera/{id}");
                    // El microservicio de Carreras acepta el token en el header "token"
                    request.Headers.Add("token", token);

                    var response = await client.SendAsync(request);
                    if (!response.IsSuccessStatusCode)
                    {
                        _logger.LogWarning("Carrera {Id} devolvió {Status}", id, response.StatusCode);
                        return null;
                    }

                    var json = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);
                    // El campo se llama "nombreCarrera" según la respuesta de ejemplo
                    if (doc.RootElement.TryGetProperty("nombreCarrera", out var prop))
                        return prop.GetString();

                    _logger.LogWarning("Carrera {Id}: campo 'nombreCarrera' no encontrado en la respuesta", id);
                    return null;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error resolviendo carrera {Id}", id);
                    return null;
                }
            });

            var resultados = await Task.WhenAll(tareas);
            // Filtrar nulos (IDs que fallaron) para no incluirlos en la respuesta
            nombres.AddRange(resultados.Where(n => n is not null)!);
            return nombres;
        }

        // Llama a GET /api/area/{id} por cada ID y recoge el nombreAreaTrab.
        // Misma estrategia de paralelismo y tolerancia a fallos parciales que en carreras.
        private async Task<List<string>> ResolverNombresAreasAsync(List<Guid> ids, string token)
        {
            if (ids.Count == 0) return new();

            var baseUrl = _configuration["MicroServicios:AreasUrl"]!.TrimEnd('/');
            var client = _httpClientFactory.CreateClient();
            var nombres = new List<string>();

            var tareas = ids.Select(async id =>
            {
                try
                {
                    var request = new HttpRequestMessage(
                        HttpMethod.Get, $"{baseUrl}/api/area/{id}");
                    // El microservicio de Áreas acepta el token en el header "token"
                    request.Headers.Add("token", token);

                    var response = await client.SendAsync(request);
                    if (!response.IsSuccessStatusCode)
                    {
                        _logger.LogWarning("Área {Id} devolvió {Status}", id, response.StatusCode);
                        return null;
                    }

                    var json = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);
                    // El campo se llama "nombreAreaTrab" según la respuesta de ejemplo
                    if (doc.RootElement.TryGetProperty("nombreAreaTrab", out var prop))
                        return prop.GetString();

                    _logger.LogWarning("Área {Id}: campo 'nombreAreaTrab' no encontrado en la respuesta", id);
                    return null;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error resolviendo área {Id}", id);
                    return null;
                }
            });

            var resultados = await Task.WhenAll(tareas);
            nombres.AddRange(resultados.Where(n => n is not null)!);
            return nombres;
        }

        private async Task<bool> ValidarTokenAsync(string token)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var request = new HttpRequestMessage(
                    HttpMethod.Get, _configuration["MicroServicios:AuthValidateUrl"]!);
                request.Headers.Add("token", token);
                var response = await client.SendAsync(request);
                if (!response.IsSuccessStatusCode) return false;
                return (await response.Content.ReadAsStringAsync())
                    .Contains("true", StringComparison.OrdinalIgnoreCase);
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
                var request = new HttpRequestMessage(
                    HttpMethod.Post, _configuration["MicroServicios:BitacoraUrl"]!)
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
