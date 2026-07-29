using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace SitioAdministrativoCarnetDigital.Services.MicroServicioFotografias
{
    public class FotografiasApiClient : IFotografiasApiClient
    {
        // HttpClient tipado cuya BaseAddress apunta a FotografiasUrl en appsettings.
        private readonly HttpClient _httpClient;

        private static readonly JsonSerializerOptions _json =
            new() { PropertyNameCaseInsensitive = true };

        public FotografiasApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // GET /api/usuario/fotografia/{correo}
        // 200 → devuelve FotografiaDto (fotoBase64 puede ser null si no tiene foto)
        // 404 → el correo no existe en el sistema
        public async Task<ApiResult<FotografiaDto>> ObtenerFotografiaAsync(
            string token, string correo)
        {
            try
            {
                // El correo va en la ruta; se codifica para manejar el @ correctamente.
                var url = $"api/usuario/fotografia/{Uri.EscapeDataString(correo)}";
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                SetBearer(request, token);

                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                    return ApiResult<FotografiaDto>.Ok(
                        await ReadJson<FotografiaDto>(response) ?? new FotografiaDto());

                return ApiResult<FotografiaDto>.Fail(
                    (int)response.StatusCode, await ReadError(response));
            }
            catch (Exception ex)
            {
                return ApiResult<FotografiaDto>.Fail(500, $"Error de conexión: {ex.Message}");
            }
        }

        // PATCH /api/usuario/fotografia
        // Headers: Authorization Bearer + Content-Type application/json
        // Body: { "identificador": correo, "fotografia": base64 }
        public async Task<ApiResult<bool>> ActualizarFotografiaAsync(
            string token, string correo, string fotoBase64)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Patch, "api/usuario/fotografia");
                SetBearer(request, token);

                var body = JsonSerializer.Serialize(new ActualizarFotografiaRequest
                {
                    Identificador = correo,
                    Fotografia = fotoBase64
                });
                request.Content = new StringContent(body, Encoding.UTF8, "application/json");

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

        // DELETE /api/usuario/fotografia
        // Header: identificador = correo (además del Authorization Bearer)
        public async Task<ApiResult<bool>> EliminarFotografiaAsync(
            string token, string correo)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Delete, "api/usuario/fotografia");
                SetBearer(request, token);
                request.Headers.Add("identificador", correo);

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

        // ── Helpers ──────────────────────────────────────────────────────────

        private static void SetBearer(HttpRequestMessage req, string token)
            => req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        private static async Task<T?> ReadJson<T>(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(content)) return default;
            try { return JsonSerializer.Deserialize<T>(content, _json); }
            catch { return default; }
        }

        // Extrae el mensaje del body en respuestas 4XX / 5XX.
        private static async Task<string> ReadError(HttpResponseMessage response)
        {
            try
            {
                var content = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(content))
                    return $"Error {(int)response.StatusCode}: {response.ReasonPhrase}";

                using var doc = JsonDocument.Parse(content);
                var root = doc.RootElement;
                if (root.TryGetProperty("message", out var m)) return m.GetString() ?? content;
                if (root.TryGetProperty("Message", out var m2)) return m2.GetString() ?? content;
                if (root.TryGetProperty("error", out var m3)) return m3.GetString() ?? content;
                return content;
            }
            catch
            {
                return $"Error {(int)response.StatusCode}: {response.ReasonPhrase}";
            }
        }
    }
}

