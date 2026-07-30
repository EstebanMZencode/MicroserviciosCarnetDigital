using System.Net.Http.Json;
using System.Text.Json;

namespace SitioAdministrativoCarnetDigital.Services.MicroServicioEstadosUsuario
{
    public class EstadosUsuarioApiClient : IEstadosUsuarioApiClient
    {
        private readonly HttpClient _http;

        public EstadosUsuarioApiClient(HttpClient http)
        {
            _http = http;
        }

        public void SetToken(string token)
        {
            if (_http.DefaultRequestHeaders.Contains("token"))
                _http.DefaultRequestHeaders.Remove("token");
            _http.DefaultRequestHeaders.Add("token", token);
        }

        public async Task CambiarEstadoAsync(string emailUsuario, Guid estadoId)
        {
            var request = new HttpRequestMessage(HttpMethod.Patch, "");
            request.Headers.Add("EmailUsuario", emailUsuario);
            request.Headers.Add("EstadoID", estadoId.ToString());

            var response = await _http.SendAsync(request);
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