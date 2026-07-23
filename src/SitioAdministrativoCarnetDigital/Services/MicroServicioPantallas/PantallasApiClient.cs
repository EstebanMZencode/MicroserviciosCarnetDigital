using System.Net.Http.Json;

namespace SitioAdministrativoCarnetDigital.Services.MicroServicioPantallas
{
    public class PantallasApiClient : IPantallasApiClient
    {
        private readonly HttpClient _http;

        public PantallasApiClient(HttpClient http)
        {
            _http = http;
        }

        public void SetToken(string token)
        {
            if (!_http.DefaultRequestHeaders.Contains("token"))
                _http.DefaultRequestHeaders.Add("token", token);
        }

        public async Task<PantallaPagedResult> GetAllAsync(int page = 1, int pageSize = 15, string? search = null)
        {
            var baseUrl = _http.BaseAddress!.ToString().TrimEnd('/');
            var url = $"{baseUrl}/pantallas?pageNumber={page}&pageSize={pageSize}";
            if (!string.IsNullOrWhiteSpace(search))
                url += $"&searchTerm={Uri.EscapeDataString(search)}";
            var resp = await _http.GetAsync(url);
            resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadFromJsonAsync<PantallaPagedResult>() ?? new();
        }

        public async Task<PantallaDto?> GetByIdAsync(Guid id)
        {
            var baseUrl = _http.BaseAddress!.ToString().TrimEnd('/');
            var resp = await _http.GetAsync($"{baseUrl}/pantallas/{id}");
            if (!resp.IsSuccessStatusCode) return null;
            return await resp.Content.ReadFromJsonAsync<PantallaDto>();
        }

        public async Task<(bool ok, string? error)> CreateAsync(PantallaDto dto)
        {
            var baseUrl = _http.BaseAddress!.ToString().TrimEnd('/');
            var resp = await _http.PostAsJsonAsync($"{baseUrl}/pantallas", dto);
            if (resp.IsSuccessStatusCode) return (true, null);
            var body = await resp.Content.ReadAsStringAsync();
            return (false, body);
        }

        public async Task<(bool ok, string? error)> UpdateAsync(PantallaDto dto)
        {
            var baseUrl = _http.BaseAddress!.ToString().TrimEnd('/');
            var resp = await _http.PutAsJsonAsync($"{baseUrl}/pantallas/{dto.PantallaID}", dto);
            if (resp.IsSuccessStatusCode) return (true, null);
            var body = await resp.Content.ReadAsStringAsync();
            return (false, body);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var baseUrl = _http.BaseAddress!.ToString().TrimEnd('/');
            var resp = await _http.DeleteAsync($"{baseUrl}/pantallas/{id}");
            return resp.IsSuccessStatusCode;
        }
    }
}