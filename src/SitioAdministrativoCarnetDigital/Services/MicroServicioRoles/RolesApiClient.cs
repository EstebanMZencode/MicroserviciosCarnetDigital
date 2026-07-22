using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace SitioAdministrativoCarnetDigital.Services.MicroServicioRoles
{
    public class RolesApiClient : IRolesApiClient
    {
        // HttpClient tipado configurado con la BaseAddress de MicroServicioRoles (RolesUrl).
        private readonly HttpClient    _httpClient;
        // IConfiguration para leer PantallasUrl y consumirla con un HttpClient temporal.
        private readonly IConfiguration _config;
        // IHttpClientFactory para crear el HttpClient de pantallas sin afectar el tipado de roles.
        private readonly IHttpClientFactory _clientFactory;

        private static readonly JsonSerializerOptions _json =
            new() { PropertyNameCaseInsensitive = true };

        public RolesApiClient(
            HttpClient httpClient,
            IConfiguration config,
            IHttpClientFactory clientFactory)
        {
            _httpClient    = httpClient;
            _config        = config;
            _clientFactory = clientFactory;
        }

        // ── GET /api/rol ─────────────────────────────────────────────────────

        public async Task<ApiResult<RolPaginadoDto>> GetRolesAsync(
            string token, int pagina = 1, int tamano = 10)
        {
            try
            {
                var request = new HttpRequestMessage(
                    HttpMethod.Get, $"api/rol?pagina={pagina}&tamano={tamano}");
                SetBearer(request, token);

                var response = await _httpClient.SendAsync(request);
                if (response.IsSuccessStatusCode)
                    return ApiResult<RolPaginadoDto>.Ok(
                        await ReadJson<RolPaginadoDto>(response) ?? new());

                return ApiResult<RolPaginadoDto>.Fail(
                    (int)response.StatusCode, await ReadError(response));
            }
            catch (Exception ex)
            {
                return ApiResult<RolPaginadoDto>.Fail(500, $"Error de conexión: {ex.Message}");
            }
        }

        // ── GET /api/rol/{id} ────────────────────────────────────────────────

        public async Task<ApiResult<RolDto>> GetRolAsync(string token, Guid rolId)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"api/rol/{rolId}");
                SetBearer(request, token);

                var response = await _httpClient.SendAsync(request);
                if (response.IsSuccessStatusCode)
                    return ApiResult<RolDto>.Ok(
                        await ReadJson<RolDto>(response) ?? new());

                return ApiResult<RolDto>.Fail(
                    (int)response.StatusCode, await ReadError(response));
            }
            catch (Exception ex)
            {
                return ApiResult<RolDto>.Fail(500, $"Error de conexión: {ex.Message}");
            }
        }

        // ── POST /api/rol ────────────────────────────────────────────────────

        public async Task<ApiResult<bool>> CrearRolAsync(
            string token, Guid rolId, string nombreRol, List<Guid> pantallas)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, "api/rol");
                SetBearer(request, token);
                request.Headers.Add("identificador", rolId.ToString());
                request.Headers.Add("nombre_rol",    nombreRol);
                request.Content = JsonBody(pantallas);

                var response = await _httpClient.SendAsync(request);
                if (response.IsSuccessStatusCode)
                    return ApiResult<bool>.Ok(true, (int)response.StatusCode);

                return ApiResult<bool>.Fail(
                    (int)response.StatusCode, await ReadError(response));
            }
            catch (Exception ex)
            {
                return ApiResult<bool>.Fail(500, $"Error de conexión: {ex.Message}");
            }
        }

        // ── PUT /api/rol ─────────────────────────────────────────────────────

        public async Task<ApiResult<bool>> ActualizarRolAsync(
            string token, Guid rolId, string nombreRol, List<Guid> pantallas)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Put, "api/rol");
                SetBearer(request, token);
                request.Headers.Add("identificador", rolId.ToString());
                request.Headers.Add("nombre_rol",    nombreRol);
                request.Content = JsonBody(pantallas);

                var response = await _httpClient.SendAsync(request);
                if (response.IsSuccessStatusCode)
                    return ApiResult<bool>.Ok(true, (int)response.StatusCode);

                return ApiResult<bool>.Fail(
                    (int)response.StatusCode, await ReadError(response));
            }
            catch (Exception ex)
            {
                return ApiResult<bool>.Fail(500, $"Error de conexión: {ex.Message}");
            }
        }

        // ── DELETE /api/rol ──────────────────────────────────────────────────

        public async Task<ApiResult<bool>> EliminarRolAsync(string token, Guid rolId)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Delete, "api/rol");
                SetBearer(request, token);
                request.Headers.Add("identificador", rolId.ToString());

                var response = await _httpClient.SendAsync(request);
                if (response.IsSuccessStatusCode)
                    return ApiResult<bool>.Ok(true, (int)response.StatusCode);

                return ApiResult<bool>.Fail(
                    (int)response.StatusCode, await ReadError(response));
            }
            catch (Exception ex)
            {
                return ApiResult<bool>.Fail(500, $"Error de conexión: {ex.Message}");
            }
        }

        // ── GET pantallas (MicroServicioPantallas) ───────────────────────────
        // Se consume desde aquí para mantener todo en MicroServicioRoles.
        // Usa IHttpClientFactory para crear un cliente sin BaseAddress tipada,
        // y construye la URL completa desde appsettings ("MicroServicios:PantallasUrl").
        // El header requerido por ese microservicio es "token", NO Authorization Bearer.

        public async Task<ApiResult<List<PantallaDto>>> GetPantallasAsync(
            string token, int pageSize = 200)
        {
            try
            {
                var baseUrl = _config["MicroServicios:PantallasUrl"]
                    ?? throw new InvalidOperationException(
                           "MicroServicios:PantallasUrl no está configurado en appsettings.");

                var url = $"{baseUrl.TrimEnd('/')}/pantallas" +
                          $"?pagenumber=1&pagesize={pageSize}&sortDirection=ASC&incluirEliminados=false";

                // Cliente sin BaseAddress tipada para no interferir con _httpClient de Roles.
                var client  = _clientFactory.CreateClient();
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("token", token);   // MicroServicioPantallas espera "token", no Bearer

                var response = await client.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    var paginado = await ReadJson<PantallasPaginadoDto>(response);
                    return ApiResult<List<PantallaDto>>.Ok(paginado?.Items ?? new());
                }

                return ApiResult<List<PantallaDto>>.Fail(
                    (int)response.StatusCode, await ReadError(response));
            }
            catch (Exception ex)
            {
                return ApiResult<List<PantallaDto>>.Fail(500, $"Error de conexión: {ex.Message}");
            }
        }

        // ── Helpers privados ─────────────────────────────────────────────────

        // Agrega el header Authorization: Bearer {token} a la solicitud.
        private static void SetBearer(HttpRequestMessage req, string token)
            => req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Serializa la lista de GUIDs de pantallas como JSON array de strings.
        private static StringContent JsonBody(List<Guid> pantallas)
        {
            var body = JsonSerializer.Serialize(pantallas.Select(g => g.ToString()).ToList());
            return new StringContent(body, Encoding.UTF8, "application/json");
        }

        // Deserializa el JSON de la respuesta con tolerancia a mayúsculas/minúsculas.
        private static async Task<T?> ReadJson<T>(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(content, _json);
        }

        // Extrae el mensaje de error del Body de respuestas 4XX / 5XX.
        // Intenta leer el campo "message" del JSON; si falla devuelve el texto plano.
        private static async Task<string> ReadError(HttpResponseMessage response)
        {
            try
            {
                var content = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(content))
                    return $"Error {(int)response.StatusCode}: {response.ReasonPhrase}";

                using var doc = JsonDocument.Parse(content);
                var root = doc.RootElement;
                if (root.TryGetProperty("message", out var m))  return m.GetString() ?? content;
                if (root.TryGetProperty("Message", out var m2)) return m2.GetString() ?? content;
                if (root.TryGetProperty("error",   out var m3)) return m3.GetString() ?? content;
                return content;
            }
            catch
            {
                return $"Error {(int)response.StatusCode}: {response.ReasonPhrase}";
            }
        }
    }
}
