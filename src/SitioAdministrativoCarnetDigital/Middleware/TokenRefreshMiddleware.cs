using System.Text.Json;
using System.Text.Json.Serialization;

namespace SitioAdministrativoCarnetDigital.Middleware
{
    // Middleware que se ejecuta en cada request con sesión activa.
    //
    // REGLAS:
    //   1. Si pasaron más de 5 minutos desde el último request → cierra sesión y redirige a Login.
    //   2. Si hubo actividad en los últimos 5 min Y pasaron más de 2 min desde el último
    //      refresh → llama a /api/refresh y guarda los NUEVOS tokens en sesión.
    //   3. En cada request exitoso → actualiza la marca de tiempo del último request.
    //
    // El refresh_token que devuelve /api/refresh reemplaza al anterior en sesión,
    // reiniciando su vigencia con cada renovación mientras el usuario esté activo.
    public class TokenRefreshMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IHttpClientFactory _clientFactory;
        private readonly ILogger<TokenRefreshMiddleware> _logger;

        // Minutos de inactividad permitidos antes de cerrar la sesión.
        private const int InactividadMaximaMinutos = 5;

        // Cada cuántos minutos se renueva el token mientras el usuario esté activo.
        private const int IntervaloRefreshMinutos = 2;

        // URL fija — no usa AuthUrl del appsettings porque ese tiene el doble segmento
        // /MicroServicioAuth/MicroServicioAuth que es correcto para /api/login
        // pero no para /api/refresh si se concatena directamente.
        private const string RefreshUrl =
            "https://tiusr23pl.cuc-carrera-ti.ac.cr/MicroServicioAuth/api/refresh";

        // Claves de sesión
        private const string KeyToken = "JwtToken";
        private const string KeyRefreshToken = "RefreshToken";
        private const string KeyEmail = "UserEmail";
        private const string KeyUltimoRequest = "UltimoRequest";  // último request activo (UTC)
        private const string KeyUltimoRefresh = "UltimoRefresh";  // último /refresh exitoso (UTC)

        // Prefijos de rutas que no cuentan como actividad ni requieren sesión.
        private static readonly string[] RutasPublicas =
        {
            "/ModuloAuth/Login",
            "/ModuloAutoregistro/Autoregistro",
            "/pages-static/",
            "/lib/", "/css/", "/js/",
            "/_", "/favicon.ico", "/Error"
        };

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
            var path = context.Request.Path.Value ?? string.Empty;

            bool esPublica = RutasPublicas.Any(r =>
                path.StartsWith(r, StringComparison.OrdinalIgnoreCase));

            if (!esPublica)
            {
                var token = context.Session.GetString(KeyToken);
                var refreshToken = context.Session.GetString(KeyRefreshToken);
                var email = context.Session.GetString(KeyEmail);

                if (!string.IsNullOrEmpty(token)
                    && !string.IsNullOrEmpty(refreshToken)
                    && !string.IsNullOrEmpty(email))
                {
                    // REGLA 1: cerrar sesión si lleva más de 5 minutos sin actividad.
                    if (SesionInactiva(context))
                    {
                        _logger.LogInformation(
                            "Sesión cerrada por inactividad para {Email}.", email);
                        context.Session.Clear();
                        context.Response.Redirect("/ModuloAuth/Login?sinSesion=1");
                        return;
                    }

                    // REGLA 3: registrar este momento como último request activo.
                    context.Session.SetString(KeyUltimoRequest, DateTime.UtcNow.ToString("O"));

                    // REGLA 2: renovar si pasaron más de 2 minutos desde el último refresh.
                    if (DebeRenovar(context))
                    {
                        await RenovarTokenAsync(context, email, refreshToken);
                    }
                }
            }

            await _next(context);
        }

        // True si el último request fue hace más de InactividadMaximaMinutos.
        // Si nunca se registró (primera request tras login), no se considera inactivo.
        private static bool SesionInactiva(HttpContext context)
        {
            var str = context.Session.GetString(KeyUltimoRequest);
            if (string.IsNullOrEmpty(str)) return false;

            if (!DateTime.TryParse(str, null,
                    System.Globalization.DateTimeStyles.RoundtripKind, out var ultimo))
                return false;

            return (DateTime.UtcNow - ultimo).TotalMinutes > InactividadMaximaMinutos;
        }

        // True si nunca se renovó o pasaron más de IntervaloRefreshMinutos desde el último.
        private static bool DebeRenovar(HttpContext context)
        {
            var str = context.Session.GetString(KeyUltimoRefresh);
            if (string.IsNullOrEmpty(str)) return true;

            if (!DateTime.TryParse(str, null,
                    System.Globalization.DateTimeStyles.RoundtripKind, out var ultimo))
                return true;

            return (DateTime.UtcNow - ultimo).TotalMinutes >= IntervaloRefreshMinutos;
        }

        private async Task RenovarTokenAsync(
            HttpContext context, string email, string refreshToken)
        {
            try
            {
                var client = _clientFactory.CreateClient();
                var request = new HttpRequestMessage(HttpMethod.Post, RefreshUrl);
                request.Headers.Add("email", email);
                request.Headers.Add("refresh_token", refreshToken);

                var response = await client.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning(
                        "Refresh fallido (HTTP {Status}) para {Email}. Cerrando sesión.",
                        (int)response.StatusCode, email);
                    context.Session.Clear();
                    return;
                }

                var content = await response.Content.ReadAsStringAsync();
                var datos = JsonSerializer.Deserialize<RefreshResponseDto>(
                    content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (datos is null || string.IsNullOrEmpty(datos.AccessToken))
                {
                    _logger.LogWarning(
                        "Refresh devolvió respuesta vacía para {Email}. Manteniendo token actual.",
                        email);
                    // No cerrar sesión — puede ser error transitorio del microservicio.
                    return;
                }

                // Guardar AMBOS tokens nuevos. El nuevo RefreshToken reinicia su vigencia.
                context.Session.SetString(KeyToken, datos.AccessToken);
                context.Session.SetString(KeyRefreshToken, datos.RefreshToken);
                context.Session.SetString(KeyUltimoRefresh, DateTime.UtcNow.ToString("O"));

                _logger.LogInformation("Token renovado para {Email}.", email);
            }
            catch (Exception ex)
            {
                // Error de red transitorio: no cerrar sesión, reintentar en el próximo request.
                _logger.LogError(ex, "Error de conexión al renovar token para {Email}.", email);
            }
        }
    }

    // Respuesta del POST /api/refresh de MicroServicioAuth.
    internal class RefreshResponseDto
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = string.Empty;

        // El nuevo refresh_token que reemplaza al anterior y reinicia su vigencia.
        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; } = string.Empty;

        [JsonPropertyName("expires_in")]
        public DateTime ExpiresIn { get; set; }
    }
}

