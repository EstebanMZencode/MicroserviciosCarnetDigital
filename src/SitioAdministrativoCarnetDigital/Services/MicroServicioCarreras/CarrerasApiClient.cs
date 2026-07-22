using System.Net;
using System.Net.Http.Json;

namespace SitioAdministrativoCarnetDigital.Services.MicroServicioCarreras
{
    public class CarrerasApiClient : ICarrerasApiClient
    {
        private readonly HttpClient _http;

        public CarrerasApiClient(HttpClient http, IConfiguration config)
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

        public async Task<List<CarreraDto>> GetAllAsync(CancellationToken ct = default)
        {
            var baseUrl = _http.BaseAddress!.ToString().TrimEnd('/');
            var result = await _http.GetFromJsonAsync<List<CarreraDto>>($"{baseUrl}/api/carrera/", ct);
            return result ?? new List<CarreraDto>();
        }

        public async Task<CarreraDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            var baseUrl = _http.BaseAddress!.ToString().TrimEnd('/');
            return await _http.GetFromJsonAsync<CarreraDto>($"{baseUrl}/api/carrera/{id}", ct);
        }

        public async Task<(bool ok, HttpStatusCode status, string? message)> CreateAsync(CarreraDto carrera, CancellationToken ct = default)
        {
            var baseUrl = _http.BaseAddress!.ToString().TrimEnd('/');
            var response = await _http.PostAsJsonAsync($"{baseUrl}/api/carrera/", carrera, ct);
            string? msg = null;
            try
            {
                var content = await response.Content.ReadAsStringAsync(ct);
                var payload = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object?>>(content);
                if (payload != null)
                {
                    if (payload.TryGetValue("message", out var m)) msg = m?.ToString();
                    else if (payload.TryGetValue("error", out var e)) msg = e?.ToString();
                    else msg = content;
                }
            }
            catch { }
            return (response.IsSuccessStatusCode, response.StatusCode, msg);
        }

        public async Task<bool> UpdateAsync(CarreraDto carrera, CancellationToken ct = default)
        {
            var baseUrl = _http.BaseAddress!.ToString().TrimEnd('/');
            var response = await _http.PutAsJsonAsync($"{baseUrl}/api/carrera/{carrera.CarreraID}", carrera, ct);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var baseUrl = _http.BaseAddress!.ToString().TrimEnd('/');
            var response = await _http.DeleteAsync($"{baseUrl}/api/carrera/{id}", ct);
            return response.IsSuccessStatusCode;
        }
    }
}