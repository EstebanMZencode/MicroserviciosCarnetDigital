using System.Text;
using System.Text.Json;

namespace SitioAdministrativoCarnetDigital.Services.MicroServicioAutoregistro
{
    public class AutoregistroApiClient : IAutoregistroApiClient
    {
        // HttpClient tipado con BaseAddress = AutoregistroUrl del appsettings.
        // El microservicio usa UsePathBase("/MicroServicioAutoregistro"), por lo que
        // la URL base en appsettings debe incluir ese segmento.
        private readonly HttpClient _httpClient;

        private static readonly JsonSerializerOptions _json =
            new() { PropertyNameCaseInsensitive = true };

        public AutoregistroApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // POST /autoregistro
        // Sin token — endpoint público por diseño.
        // El microservicio responde 201 con { message } si el registro fue exitoso,
        // o 400 con { message } si hay un error de validación.
        public async Task<ApiResult<AutoregistroResponseDto>> RegistrarAsync(
            UsuarioRegistroRequest request)
        {
            try
            {
                var body = JsonSerializer.Serialize(request, _json);
                var content = new StringContent(body, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("autoregistro", content);

                if (response.IsSuccessStatusCode)
                {
                    var data = await ReadJson<AutoregistroResponseDto>(response);
                    return ApiResult<AutoregistroResponseDto>.Ok(
                        data ?? new(), (int)response.StatusCode);
                }

                return ApiResult<AutoregistroResponseDto>.Fail(
                    (int)response.StatusCode, await ReadError(response));
            }
            catch (Exception ex)
            {
                return ApiResult<AutoregistroResponseDto>.Fail(500,
                    $"Error de conexión: {ex.Message}");
            }
        }

        // GET /autoregistro/confirmar?token={token}
        // Invocado desde el enlace del correo de confirmación.
        public async Task<ApiResult<AutoregistroResponseDto>> ConfirmarAsync(string token)
        {
            try
            {
                var url = $"autoregistro/confirmar?token={Uri.EscapeDataString(token)}";
                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var data = await ReadJson<AutoregistroResponseDto>(response);
                    return ApiResult<AutoregistroResponseDto>.Ok(
                        data ?? new(), (int)response.StatusCode);
                }

                return ApiResult<AutoregistroResponseDto>.Fail(
                    (int)response.StatusCode, await ReadError(response));
            }
            catch (Exception ex)
            {
                return ApiResult<AutoregistroResponseDto>.Fail(500,
                    $"Error de conexión: {ex.Message}");
            }
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private static async Task<T?> ReadJson<T>(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(content)) return default;
            try { return JsonSerializer.Deserialize<T>(content, _json); }
            catch { return default; }
        }

        // Extrae el mensaje de error del body en respuestas 4XX / 5XX.
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
