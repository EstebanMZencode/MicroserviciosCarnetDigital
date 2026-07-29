using System.Text.Json;
using System.Text.Json.Serialization;

namespace SitioAdministrativoCarnetDigital.Middleware
{
    // Renueva el JWT automáticamente cada 2 minutos de actividad del usuario.
    // La renovación es transparente: el usuario no percibe ninguna interrupción.
    public class TokenRefreshMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IHttpClientFactory _clientFactory;
        private readonly ILogger<TokenRefreshMiddleware> _logger;

        // Intervalo fijo de renovación: cada 2 minutos de actividad.
        private const int IntervaloRenovacionMinutos = 2;

        // Clave de sesión que registra la última vez que se renovó el token.
        private const string SessionKeyUltimaRenovacion = "TokenUltimaRenovacion";

        // URL fija del endpoint refresh — no usa AuthUrl del appsettings
        // porque ese tiene el doble segmento /MicroServicioAuth/MicroServicioAuth
        // necesario para /api/login pero que rompería esta URL.
        private const string RefreshUrl =
            "https://tiusr23pl.cuc-carrera-ti.ac.cr/MicroServicioAuth/api/refresh";

        public TokenRefreshMiddleware(
            RequestDelegate next,
            IHttpClientFactory clientFactory,
            ILogger<TokenRefreshMiddleware> logger)
        {
            _next = next;
            _clientFactory = clientFactory;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var token = context.Session.GetString("JwtToken");
            var refreshToken = context.Session.GetString("RefreshToken");
            var email = context.Session.GetString("UserEmail");

            // Solo actuar si hay sesión activa con todos los datos necesarios.
            if (!string.IsNullOrEmpty(token)
                && !string.IsNullOrEmpty(refreshToken)
                && !string.IsNullOrEmpty(email)
                && HanPasadoDosMinutos(context))
            {
                await RenovarTokenAsync(context, email, refreshToken);
            }

            await _next(context);
        }

        // Retorna true si pasaron más de 2 minutos desde la última renovación.
        // Esto garantiza que se llame a /refresh cada 2 minutos de actividad,
        // sin importar la duración real del JWT.
        private static bool HanPasadoDosMinutos(HttpContext context)
        {
            var ultimaStr = context.Session.GetString(SessionKeyUltimaRenovacion);

            if (string.IsNullOrEmpty(ultimaStr)
                || !DateTime.TryParse(ultimaStr, out var ultima))
            {
                // Primera vez — renovar inmediatamente para registrar la marca de tiempo.
                return true;
            }

            return (DateTime.UtcNow - ultima).TotalMinutes >= IntervaloRenovacionMinutos;
        }

        // Llama a /api/refresh y actualiza la sesión con los nuevos tokens.
        private async Task RenovarTokenAsync(HttpContext context, string email, string refreshToken)
        {
            try
            {
                var client = _clientFactory.CreateClient();
                var request = new HttpRequestMessage(HttpMethod.Post, RefreshUrl);

                // El endpoint espera exactamente estos dos headers según la documentación.
                request.Headers.Add("email", email);
                request.Headers.Add("refresh_token", refreshToken);

                var response = await client.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    // 401 = refresh_token expirado o inválido → el usuario debe volver a loguearse.
                    _logger.LogWarning(
                        "Refresh fallido (HTTP {Status}) para {Email}. Sesión cerrada.",
                        (int)response.StatusCode, email);
                    context.Session.Clear();
                    return;
                }

                var content = await response.Content.ReadAsStringAsync();
                var datos = JsonSerializer.Deserialize<RefreshResponseDto>(
                    content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (datos is null || string.IsNullOrEmpty(datos.AccessToken))
                {
                    _logger.LogWarning(
                        "Refresh devolvió cuerpo vacío para {Email}. Se mantiene el token actual.",
                        email);
                    // No cerrar sesión — puede ser un error transitorio.
                    return;
                }

                // Actualizar sesión con los nuevos tokens y registrar la marca de tiempo.
                context.Session.SetString("JwtToken", datos.AccessToken);
                context.Session.SetString("RefreshToken", datos.RefreshToken);
                context.Session.SetString(SessionKeyUltimaRenovacion, DateTime.UtcNow.ToString("O"));

                _logger.LogInformation(
                    "Token renovado correctamente para {Email}.", email);
            }
            catch (Exception ex)
            {
                // Error de red: NO cerrar sesión. El próximo request reintentará.
                _logger.LogError(ex,
                    "Error de conexión al intentar renovar token para {Email}.", email);
            }
        }
    }

    // Respuesta del POST /api/refresh de MicroServicioAuth.
    internal class RefreshResponseDto
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = string.Empty;

        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; } = string.Empty;

        [JsonPropertyName("expires_in")]
        public DateTime ExpiresIn { get; set; }
    }
}

