using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace SitioAdministrativoCarnetDigital.Services.MicroServicioTiposIdentificacion
{
    public class TiposIdentificacionApiClient : ITiposIdentificacionApiClient
    {
        private readonly HttpClient _http;
        private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

        public TiposIdentificacionApiClient(HttpClient http)
        {
            _http = http;
        }

        public void SetToken(string token)
        {
            if (_http.DefaultRequestHeaders.Contains("token"))
                _http.DefaultRequestHeaders.Remove("token");
            _http.DefaultRequestHeaders.Add("token", token);
        }

        public async Task<List<TipoIdentificacionDto>> ObtenerTodosAsync()
        {
            var response = await _http.GetAsync("");
            await LanzarSiErrorAsync(response);
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<TipoIdentificacionDto>>(json, _jsonOptions) ?? new();
        }

        public async Task CrearAsync(string nombre)
        {
            var response = await _http.PostAsJsonAsync("", new { nombreTipoIdent = nombre });
            await LanzarSiErrorAsync(response);
        }

        public async Task ActualizarAsync(Guid id, string nombre)
        {
            var response = await _http.PutAsJsonAsync($"{id}", new { nombreTipoIdent = nombre });
            await LanzarSiErrorAsync(response);
        }

        public async Task EliminarAsync(Guid id)
        {
            var response = await _http.DeleteAsync($"{id}");
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

        private static string ExtraerMensaje(string json)
        {
            try
            {
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("message", out var msg))
                    return msg.GetString() ?? json;
            }
            catch { }
            return string.IsNullOrWhiteSpace(json) ? "Ocurrió un error (sin detalle del servidor)." : json;
        }
    }
}