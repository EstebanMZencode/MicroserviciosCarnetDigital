using System.Net.Http.Json;

namespace SitioAdministrativoCarnetDigital.Services.MicroServicioBitacoras
{
    public class BitacorasApiClient : IBitacorasApiClient
    {
        private readonly HttpClient _http;

        public BitacorasApiClient(HttpClient http, IConfiguration config)
        {
            _http = http;
            ObtenerTokenAsync(config).Wait();
        }

        private async Task ObtenerTokenAsync(IConfiguration config)
        {
            try
            {
                var authUrl = config["MicroServicios:AuthUrl"];
                var handler = new HttpClientHandler();
                handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
                using var tempClient = new HttpClient(handler);
                var request = new HttpRequestMessage(HttpMethod.Post, $"{authUrl}/api/login");
                request.Headers.Add("usuario", "jimenezArriet@gmail.com");
                request.Headers.Add("contrasena", "Sebas123");
                request.Headers.Add("tipo_usuario", "0b4d7da0-1438-436c-b2d4-2d3a8f3005e7");
                var response = await tempClient.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    var payload = await response.Content.ReadFromJsonAsync<Dictionary<string, string?>>();
                    if (payload != null && payload.TryGetValue("access_token", out var token))
                        _http.DefaultRequestHeaders.Add("token", token);
                }
            }
            catch { }
        }

        public async Task<BitacoraPagedResult> GetAllAsync(int page = 1, int pageSize = 15, string? search = null, string? token = null)
        {
            var req = new HttpRequestMessage(HttpMethod.Get,
                $"MicroServicioBitacoras/bitacora?pageNumber={page}&pageSize={pageSize}" +
                (string.IsNullOrWhiteSpace(search) ? "" : $"&searchTerm={Uri.EscapeDataString(search)}"));
            var resp = await _http.SendAsync(req);
            resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadFromJsonAsync<BitacoraPagedResult>() ?? new();
        }
    }
}