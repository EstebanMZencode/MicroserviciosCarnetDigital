using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace SitioAdministrativoCarnetDigital.Services.MicroServicioTiposUsuario
{
    // Registrado en Program.cs vía:
    // builder.Services.AddHttpClient<ITiposUsuarioApiClient, TiposUsuarioApiClient>(...)
    // El HttpClient ya viene con BaseAddress = MicroServicios:TiposUsuarioUrl (appsettings.json).
    // IMPORTANTE: esa URL debe terminar en "/" para que las rutas con {id} combinen bien.
    public class TiposUsuarioApiClient : ITiposUsuarioApiClient
    {
        private readonly HttpClient _httpClient;
        private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

        public TiposUsuarioApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public void SetToken(string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        public async Task<List<TipoUsuarioDto>> ObtenerTodosAsync()
        {
            var response = await _httpClient.GetAsync("");
            await LanzarSiErrorAsync(response);

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<TipoUsuarioDto>>(json, _jsonOptions) ?? new();
        }

        public async Task<TipoUsuarioDto> CrearAsync(string nombre)
        {
            var response = await _httpClient.PostAsJsonAsync("", new { nombreTipoUsuario = nombre });
            await LanzarSiErrorAsync(response);

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<TipoUsuarioDto>(json, _jsonOptions)!;
        }

        public async Task<TipoUsuarioDto> ActualizarAsync(Guid id, string nombre)
        {
            var response = await _httpClient.PutAsJsonAsync($"{id}", new { nombreTipoUsuario = nombre });
            await LanzarSiErrorAsync(response);

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<TipoUsuarioDto>(json, _jsonOptions)!;
        }

        public async Task EliminarAsync(Guid id)
        {
            var response = await _httpClient.DeleteAsync($"{id}");
            await LanzarSiErrorAsync(response);
        }

        private static async Task LanzarSiErrorAsync(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode) return;

            var body = await response.Content.ReadAsStringAsync();
            var mensaje = ExtraerMensaje(body);
            var uri = response.RequestMessage?.RequestUri?.ToString() ?? "(desconocida)";
            throw new HttpRequestException($"{mensaje} [URL: {uri}]", null, response.StatusCode);
        }

        // El endpoint de TiposUsuario responde { "error": "..." } en los casos de error.
        private static string ExtraerMensaje(string json)
        {
            try
            {
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("error", out var err))
                    return err.GetString() ?? "Ocurrió un error.";
            }
            catch { /* body no era JSON */ }
            return "Ocurrió un error.";
        }
    }
}