using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace SitioAdministrativoCarnetDigital.Services.MicroServicioEstadosUsuario
{
    // Registrado en Program.cs vía:
    // builder.Services.AddHttpClient<IEstadosUsuarioApiClient, EstadosUsuarioApiClient>(...)
    // BaseAddress = MicroServicios:EstadosUsuarioUrl (appsettings.json).
    // IMPORTANTE: esa URL debe apuntar a ".../MicroServicioActualizarEstado/api/usuarios/estado/"
    // (con la ruta completa y la barra final), no solo a la raíz del microservicio.
    //
    // A diferencia de TiposUsuario/TiposIdentificacion, este endpoint NO recibe
    // JSON en el body: todo va por headers (EmailUsuario, EstadoID, Authorization).
    public class EstadosUsuarioApiClient : IEstadosUsuarioApiClient
    {
        private readonly HttpClient _http;

        public EstadosUsuarioApiClient(HttpClient http, IConfiguration config)
        {
            _http = http;
            var tokenTask = ObtenerTokenAsync(config);
            tokenTask.Wait();
        }

        private async Task ObtenerTokenAsync(IConfiguration config)
        {
            try
            {
                var authUrl = config["MicroServicios:AuthUrl"];
                using var tempClient = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Post, $"{authUrl}/api/login");
                request.Headers.Add("usuario", "jimenezArriet@gmail.com");
                request.Headers.Add("contrasena", "Sebas123");
                request.Headers.Add("tipo_usuario", "0b4d7da0-1438-436c-b2d4-2d3a8f3005e7");
                var response = await tempClient.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    var payload = await response.Content.ReadFromJsonAsync<Dictionary<string, string?>>();
                    if (payload != null && payload.TryGetValue("access_token", out var token))
                        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }
            }
            catch { }
        }

        public void SetToken(string token)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
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