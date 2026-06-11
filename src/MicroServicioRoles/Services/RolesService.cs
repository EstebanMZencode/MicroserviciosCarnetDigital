using System.Text;
using System.Text.Json;
using MicroServicioRoles.Entities;
using MicroServicioRoles.Repository;

namespace MicroServicioRoles.Services
{
    public class RolesService : IRolesService
    {
        private readonly RolesRepository _rolesRepository;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<RolesService> _logger;

        public RolesService(
            RolesRepository rolesRepository,
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory,
            ILogger<RolesService> logger)
        {
            _rolesRepository = rolesRepository;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        // ── POST /api/rol ─────────────────────────────────────────────────────

        public async Task<(RolResponse? data, int statusCode, string? error)> CrearRolAsync(
            Guid rolId, string nombreRol, List<Guid> pantallas, string token)
        {
            if (!await ValidarTokenAsync(token))
                return (null, 401, "No autorizado.");

            bool existenPantallas;
            try
            {
                existenPantallas = await _rolesRepository.ExistenPantallasAsync(pantallas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error de conexion a DB en CrearRolAsync (validar pantallas).");
                return (null, 500, "No se pudo conectarse a la base de datos. Inténtelo más tarde.");
            }

            if (!existenPantallas)
                return (null, 404, "No se encontraron las pantallas solicitadas.");

            try
            {
                await _rolesRepository.CrearRolAsync(rolId, nombreRol, pantallas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error de conexion a DB en CrearRolAsync (insertar).");
                return (null, 500, "No se pudo conectarse a la base de datos. Inténtelo más tarde.");
            }

            var respuesta = new RolResponse
            {
                RolID = rolId,
                NombreRol = nombreRol,
                Pantallas = pantallas
            };

            _ = RegistrarBitacoraAsync(token, ExtraerEmail(token),
                $"Creacion de rol {rolId}: Nombre={nombreRol}, Pantallas=[{string.Join(", ", pantallas)}]");

            return (respuesta, 201, null);
        }

        // ── PUT /api/rol ──────────────────────────────────────────────────────

        public async Task<(int statusCode, string? error)> ModificarRolAsync(
            Guid rolId, string nombreRol, List<Guid> pantallas, string token)
        {
            if (!await ValidarTokenAsync(token))
                return (401, "No autorizado.");

            RolResponse? rolAntes;
            bool existenPantallas;
            bool existeRol;
            try
            {
                rolAntes = await _rolesRepository.GetRolPorIdAsync(rolId);
                existeRol = rolAntes is not null;
                existenPantallas = existeRol && await _rolesRepository.ExistenPantallasAsync(pantallas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error de conexion a DB en ModificarRolAsync (validar).");
                return (500, "No se pudo conectarse a la base de datos. Inténtelo más tarde.");
            }

            if (!existeRol)
                return (404, "No se encontró el identificador solicitado.");

            if (!existenPantallas)
                return (404, "No se encontraron las pantallas solicitadas.");

            try
            {
                await _rolesRepository.ActualizarRolAsync(rolId, nombreRol, pantallas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error de conexion a DB en ModificarRolAsync (actualizar).");
                return (500, "No se pudo conectarse a la base de datos. Inténtelo más tarde.");
            }

            var pantallasAnteriores = rolAntes.Pantallas.Count > 0
                ? $"[{string.Join(", ", rolAntes.Pantallas)}]"
                : "[]";
            var pantallasNuevas = pantallas.Count > 0
                ? $"[{string.Join(", ", pantallas)}]"
                : "[]";
            _ = RegistrarBitacoraAsync(token, ExtraerEmail(token),
                $"Modificacion de rol {rolId}: Nombre={nombreRol}, Pantallas anteriores={pantallasAnteriores}, Pantallas nuevas={pantallasNuevas}");


            return (204, null);
        }

        // ── DELETE /api/rol ───────────────────────────────────────────────────

        public async Task<(int statusCode, string? error)> EliminarRolAsync(
            Guid rolId, string token)
        {
            if (!await ValidarTokenAsync(token))
                return (401, "No autorizado.");

            RolResponse? rol;
            try
            {
                rol = await _rolesRepository.GetRolPorIdAsync(rolId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error de conexion a DB en EliminarRolAsync (buscar).");
                return (500, "No se pudo conectarse a la base de datos. Inténtelo más tarde.");
            }

            if (rol is null)
                return (404, "No se encontró el identificador solicitado.");

            try
            {
                await _rolesRepository.EliminarRolAsync(rolId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error de conexion a DB en EliminarRolAsync (eliminar).");
                return (500, "No se pudo conectarse a la base de datos. Inténtelo más tarde.");
            }

            var pantallasEliminadas = rol.Pantallas.Count > 0
                ? $"[{string.Join(", ", rol.Pantallas)}]"
                : "[]";
            _ = RegistrarBitacoraAsync(token, ExtraerEmail(token),
                $"Eliminacion de rol {rolId}: Nombre={rol.NombreRol}, Pantallas={pantallasEliminadas}");


            return (204, null);
        }

        // ── GET /api/rol ──────────────────────────────────────────────────────

        public async Task<(RolPaginadoResponse? data, int statusCode, string? error)> ObtenerRolesAsync(
            int pagina, int tamano, string token)
        {
            if (!await ValidarTokenAsync(token))
                return (null, 401, "No autorizado.");

            int total;
            List<RolResponse> roles;
            try
            {
                total = await _rolesRepository.GetTotalRolesAsync();
                roles = await _rolesRepository.GetRolesPaginadosAsync(pagina, tamano);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error de conexion a DB en ObtenerRolesAsync.");
                return (null, 500, "No se pudo conectarse a la base de datos. Inténtelo más tarde.");
            }

            if (!roles.Any())
                return (null, 404, "No se encontró ningún registro para roles.");

            var respuesta = new RolPaginadoResponse
            {
                TotalRegistros = total,
                Pagina = pagina,
                Tamano = tamano,
                Roles = roles
            };

            var detallesRoles = string.Join(" | ", roles.Select(r =>
                $"Rol={r.NombreRol} (ID={r.RolID}, Pantallas=[{string.Join(", ", r.Pantallas)}])"));
            _ = RegistrarBitacoraAsync(token, ExtraerEmail(token),
                $"Consulta paginada de roles (pagina {pagina}, tamano {tamano}): {detallesRoles}");


            return (respuesta, 200, null);
        }

        // ── GET /api/rol/{id} ─────────────────────────────────────────────────

        public async Task<(RolResponse? data, int statusCode, string? error)> ObtenerRolPorIdAsync(
            Guid rolId, string token)
        {
            if (!await ValidarTokenAsync(token))
                return (null, 401, "No autorizado.");

            RolResponse? rol;
            try
            {
                rol = await _rolesRepository.GetRolPorIdAsync(rolId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error de conexion a DB en ObtenerRolPorIdAsync.");
                return (null, 500, "No se pudo conectarse a la base de datos. Inténtelo más tarde.");
            }

            if (rol is null)
                return (null, 404, "No se encontró el rol solicitado.");

            _ = RegistrarBitacoraAsync(token, ExtraerEmail(token),
                $"Consulta de rol {rolId}: Nombre={rol.NombreRol}, Pantallas=[{string.Join(", ", rol.Pantallas)}]");

            return (rol, 200, null);
        }

        // ── Métodos privados ──────────────────────────────────────────────────

        // Llama a GET /api/validate del MicroServicioAuth para validar el JWT.
        // Retorna false si el servicio no responde, el token es inválido o está expirado.
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
                _logger.LogWarning(ex, "No se pudo contactar al servicio de validacion de token.");
                return false;
            }
        }

        // Extrae el email del claim 'sub' o 'email' del JWT sin validar firma.
        // Si no puede extraerlo retorna string vacío.
        private static string ExtraerEmail(string token)
        {
            try
            {
                var payload = token.Split('.')[1];
                var padding = (4 - payload.Length % 4) % 4;
                var padded = payload + new string('=', padding);
                var json = Encoding.UTF8.GetString(Convert.FromBase64String(padded));

                var claims = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);
                if (claims is null) return string.Empty;

                if (claims.TryGetValue("sub", out var sub)) return sub.GetString() ?? string.Empty;
                if (claims.TryGetValue("email", out var email)) return email.GetString() ?? string.Empty;
                return string.Empty;
            }
            catch { return string.Empty; }
        }

        // Registra la acción en MicroServicioBitacoras (fire-and-forget).
        // Los errores se loguean pero no afectan la respuesta principal.
        private async Task RegistrarBitacoraAsync(string token, string email, string descripcion)
        {
            try
            {
                var bitacoraUrl = _configuration["MicroServicios:BitacoraUrl"];
                var client = _httpClientFactory.CreateClient();

                var body = JsonSerializer.Serialize(new { usuarioID = email, descripcion });

                var request = new HttpRequestMessage(HttpMethod.Post, bitacoraUrl)
                {
                    Content = new StringContent(body, Encoding.UTF8, "application/json")
                };

                request.Headers.Add("token", token);

                await client.SendAsync(request);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error al registrar en bitacora para el usuario '{Email}'.", email);
            }
        }
    }
}
