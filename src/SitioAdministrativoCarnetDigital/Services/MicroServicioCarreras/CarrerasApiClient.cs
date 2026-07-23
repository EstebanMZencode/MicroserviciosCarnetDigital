using System.Net;
using System.Net.Http.Json;

namespace SitioAdministrativoCarnetDigital.Services.MicroServicioCarreras
{
    public class CarrerasApiClient : ICarrerasApiClient
    {
        private readonly HttpClient _http;

        public CarrerasApiClient(HttpClient http)
        {
            _http = http;
        }

        public void SetToken(string token)
        {
            if (!_http.DefaultRequestHeaders.Contains("token"))
                _http.DefaultRequestHeaders.Add("token", token);
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