using MicroServicioAuth.Entities;
using MicroServicioAuth.Repository;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;

namespace MicroServicioAuth.Services
{
    /// <summary>
    /// Implementación del servicio de autenticación.
    /// Orquesta la validación de credenciales, generación de JWT y RefreshToken,
    /// consulta de parámetros y registro en bitácora.
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly AuthRepository _authRepository;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            AuthRepository authRepository,
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory,
            ILogger<AuthService> logger)
        {
            _authRepository = authRepository;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        // ── /login ────────────────────────────────────────────────────────────

        /// <inheritdoc/>
        public async Task<(LoginResponse? data, int statusCode, string? error)> LoginAsync(
            string email, string password, string tipoUsuario)
        {
            // Validar que tipo_usuario sea un GUID válido antes de consultar la DB
            if (!Guid.TryParse(tipoUsuario, out var tipoUsuarioId))
                return (null, 400, "El tipo de usuario ingresado no es válido.");

            // Consultar credenciales en base de datos
            UserLoginData? loginData;
            try
            {
                loginData = await _authRepository.GetLoginDataAsync(email, tipoUsuarioId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al conectarse a la base de datos en LoginAsync.");
                return (null, 500, "No se pudo conectarse a la base de datos. Inténtelo más tarde.");
            }

            // Verificar que el usuario exista y que la contraseña coincida con el hash BCrypt
            if (loginData is null || !BCrypt.Net.BCrypt.Verify(password, loginData.PasswordHash))
                return (null, 401, "Usuario y/o contraseña incorrectos.");

            // Recuperar duraciones desde MicroServicioParametros (o usar valores por defecto)
            var jwtMinutos = await GetParametroMinutosAsync("JWT_TOKEN") ?? 5;
            var refreshMinutos = await GetParametroMinutosAsync("REFRESH") ?? (5 + jwtMinutos);

            // Generar JWT y RefreshToken
            var jwtExpiracion = DateTime.UtcNow.AddMinutes(jwtMinutos);
            var jwtToken = GenerarJwtToken(loginData.Email, loginData.RolID, loginData.TipoUsuarioID, jwtExpiracion);
            var refreshToken = GenerarRefreshToken();
            var refreshExpiracion = DateTime.UtcNow.AddMinutes(refreshMinutos);

            // Persistir RefreshToken (upsert)
            try
            {
                await _authRepository.UpsertRefreshTokenAsync(email, refreshToken, refreshExpiracion);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al persistir el RefreshToken en base de datos.");
                return (null, 500, "No se pudo conectarse a la base de datos. Inténtelo más tarde.");
            }

            // Registrar en bitácora (fire-and-forget, no bloquea la respuesta)
            _ = RegistrarBitacoraAsync(jwtToken, email, "Inició sesión exitosamente.");

            return (new LoginResponse
            {
                expires_in = jwtExpiracion,
                access_token = jwtToken,
                refresh_token = refreshToken,
                usuarioID = email,
                rol = loginData.RolID
            }, 201, null);
        }

        // ── /refresh ──────────────────────────────────────────────────────────

        /// <inheritdoc/>
        public async Task<(RefreshResponse? data, int statusCode, string? error)> RefreshAsync(
            string email, string refreshToken)
        {
            // Validar RefreshToken contra la base de datos
            RefreshTokenRecord? tokenRecord;
            try
            {
                tokenRecord = await _authRepository.GetActiveRefreshTokenAsync(email, refreshToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al conectarse a la base de datos en RefreshAsync.");
                return (null, 500, "No se pudo conectarse a la base de datos. Inténtelo más tarde.");
            }

            // Si no existe, no coincide o expiró → 401
            if (tokenRecord is null)
                return (null, 401, "No autorizado.");

            // Recuperar duraciones desde MicroServicioParametros (o usar valores por defecto)
            var jwtMinutos = await GetParametroMinutosAsync("JWTTOKEN") ?? 5;
            var refreshMinutos = await GetParametroMinutosAsync("REFRESH") ?? (5 + jwtMinutos);

            // Generar nuevos tokens
            var jwtExpiracion = DateTime.UtcNow.AddMinutes(jwtMinutos);
            var nuevoJwtToken = GenerarJwtToken(email, Guid.Empty, Guid.Empty, jwtExpiracion);
            var nuevoRefreshToken = GenerarRefreshToken();
            var refreshExpiracion = DateTime.UtcNow.AddMinutes(refreshMinutos);

            // Actualizar RefreshToken en base de datos
            try
            {
                await _authRepository.UpsertRefreshTokenAsync(email, nuevoRefreshToken, refreshExpiracion);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar el RefreshToken en base de datos.");
                return (null, 500, "No se pudo conectarse a la base de datos. Inténtelo más tarde.");
            }

            // Registrar en bitácora (fire-and-forget)
            _ = RegistrarBitacoraAsync(nuevoJwtToken, email, "Generó nuevos tokens de acceso.");

            return (new RefreshResponse
            {
                expires_in = jwtExpiracion,
                access_token = nuevoJwtToken,
                refresh_token = nuevoRefreshToken
            }, 201, null);
        }

        // ── /validate ─────────────────────────────────────────────────────────

        /// <inheritdoc/>
        public (bool isValid, int statusCode, string message) ValidateToken(string token)
        {
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));

            try
            {
                new JwtSecurityTokenHandler().ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                    ValidateIssuer = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = _configuration["Jwt:Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero   // sin margen de tolerancia
                }, out _);

                return (true, 200, "true");
            }
            catch
            {
                return (false, 401, "No autorizado.");
            }
        }

        // ── Métodos privados ──────────────────────────────────────────────────

        /// <summary>
        /// Genera un JWT firmado con HMAC-SHA256 que incluye email, RolID y TipoUsuarioID
        /// como claims. Los GUIDs vacíos se omiten del token (caso refresh).
        /// </summary>
        private string GenerarJwtToken(string email, Guid rolId, Guid tipoUsuarioId, DateTime expiracion)
        {
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub,   email),
                new(JwtRegisteredClaimNames.Email, email),
                new(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString())
            };

            if (rolId != Guid.Empty)
                claims.Add(new Claim("rol", rolId.ToString()));

            if (tipoUsuarioId != Guid.Empty)
                claims.Add(new Claim("tipo_usuario", tipoUsuarioId.ToString()));

            var clave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var credenciales = new SigningCredentials(clave, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: expiracion,
                signingCredentials: credenciales);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// Genera un token de refresco criptográficamente seguro de 64 bytes (Base64, 88 chars).
        /// Garantiza unicidad estadística sin depender de la base de datos.
        /// </summary>
        private static string GenerarRefreshToken()
            => Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

        /// <summary>
        /// Consulta el MicroServicioParametros para obtener la duración en minutos del parámetro dado.
        /// Retorna <c>null</c> si el servicio no responde o el valor no es un entero válido.
        /// </summary>
        /// <param name="identificador">Clave del parámetro: "JWT_TOKEN" o "REFRESH".</param>
        private async Task<int?> GetParametroMinutosAsync(string identificador)
        {
            try
            {
                var baseUrl = _configuration["MicroServicios:ParametrosBaseUrl"];
                var client = _httpClientFactory.CreateClient();

                var response = await client.GetAsync($"{baseUrl}/parametro/{identificador}");

                if (!response.IsSuccessStatusCode)
                    return null;

                var json = await response.Content.ReadAsStringAsync();
                var parametro = JsonSerializer.Deserialize<ParametroValor>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return (parametro is not null && int.TryParse(parametro.Valor, out var minutos))
                    ? minutos
                    : null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "No se pudo obtener el parámetro '{Id}'. Se usará la duración por defecto.", identificador);
                return null;
            }
        }

        /// <summary>
        /// Registra la acción en el MicroServicioBitacoras (fire-and-forget).
        /// Los errores se loguean pero no afectan la respuesta del endpoint principal.
        /// Envía el JWT en el header Authorization y el cuerpo en formato JSON.
        /// </summary>
        /// <param name="jwtToken">JWT generado en este request, enviado como Bearer token.</param>
        /// <param name="email">Email del usuario que ejecutó la acción.</param>
        /// <param name="descripcion">Descripción legible de la acción realizada.</param>
        private async Task RegistrarBitacoraAsync(string jwtToken, string email, string descripcion)
        {
            try
            {
                var bitacoraUrl = _configuration["MicroServicios:BitacoraUrl"];
                var client = _httpClientFactory.CreateClient();

                var body = JsonSerializer.Serialize(new BitacoraRequest
                {
                    usuarioID = email,
                    descripcion = descripcion
                });

                var request = new HttpRequestMessage(HttpMethod.Post, bitacoraUrl)
                {
                    Content = new StringContent(body, Encoding.UTF8, "application/json")
                };

                request.Headers.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwtToken);

                await client.SendAsync(request);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error al registrar en bitácora para el usuario '{Email}'.", email);
            }
        }
    }
}

