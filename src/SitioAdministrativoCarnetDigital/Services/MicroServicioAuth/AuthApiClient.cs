using System.Text.Json;

namespace SitioAdministrativoCarnetDigital.Services.MicroServicioAuth
{
    public class AuthApiClient : IAuthApiClient
    {
        private readonly HttpClient _httpClient;

        private static readonly JsonSerializerOptions _json =
            new() { PropertyNameCaseInsensitive = true };

        public AuthApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // POST {AuthUrl}/api/login
        // URL base configurada en appsettings: MicroServicios:AuthUrl
        //   = https://tiusr23pl.cuc-carrera-ti.ac.cr/MicroServicioAuth
        // Ruta relativa: api/login  →  URL completa: .../MicroServicioAuth/api/login
        // Las credenciales van exclusivamente en headers HTTP; el body va vacío.
        // El microservicio responde 201 en éxito con LoginResponseDto.
        public async Task<ApiResult<LoginResponseDto>> LoginAsync(
            string usuario,
            string contrasena,
            string tipoUsuario)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, "api/login");
                request.Headers.Add("usuario", usuario.Trim());
                request.Headers.Add("contrasena", contrasena);
                request.Headers.Add("tipo_usuario", tipoUsuario.Trim());
                // Sin body: el endpoint solo lee headers.

                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var data = await ReadJson<LoginResponseDto>(response);
                    return ApiResult<LoginResponseDto>.Ok(
                        data ?? new LoginResponseDto(), (int)response.StatusCode);
                }

                return ApiResult<LoginResponseDto>.Fail(
                    (int)response.StatusCode, await ReadError(response));
            }
            catch (Exception ex)
            {
                return ApiResult<LoginResponseDto>.Fail(500,
                    $"No se pudo conectar al servidor de autenticación: {ex.Message}");
            }
        }

        private static async Task<T?> ReadJson<T>(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();
            return string.IsNullOrWhiteSpace(content)
                ? default
                : JsonSerializer.Deserialize<T>(content, _json);
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

