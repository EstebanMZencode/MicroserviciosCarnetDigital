using System.Net.Http.Json;

namespace SitioAdministrativoCarnetDigital.Services.MicroServicioPantallas
{
    public class PantallasApiClient : IPantallasApiClient
    {
        private readonly HttpClient _http;
        public PantallasApiClient(HttpClient http) => _http = http;

        public async Task<PantallaPagedResult> GetAllAsync(int page = 1, int pageSize = 15, string? search = null, string? token = null)
        {
            var req = new HttpRequestMessage(HttpMethod.Get,
                $"pantallas?pageNumber={page}&pageSize={pageSize}" +
                (string.IsNullOrWhiteSpace(search) ? "" : $"&searchTerm={Uri.EscapeDataString(search)}"));
            if (token != null) req.Headers.Add("token", token);
            var resp = await _http.SendAsync(req);
            resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadFromJsonAsync<PantallaPagedResult>() ?? new();
        }

        public async Task<PantallaDto?> GetByIdAsync(Guid id, string? token = null)
        {
            var req = new HttpRequestMessage(HttpMethod.Get, $"pantallas/{id}");
            if (token != null) req.Headers.Add("token", token);
            var resp = await _http.SendAsync(req);
            if (!resp.IsSuccessStatusCode) return null;
            return await resp.Content.ReadFromJsonAsync<PantallaDto>();
        }

        public async Task<(bool ok, string? error)> CreateAsync(PantallaDto dto, string? token = null)
        {
            var req = new HttpRequestMessage(HttpMethod.Post, "pantallas");
            req.Content = JsonContent.Create(dto);
            if (token != null) req.Headers.Add("token", token);
            var resp = await _http.SendAsync(req);
            if (resp.IsSuccessStatusCode) return (true, null);
            var body = await resp.Content.ReadAsStringAsync();
            return (false, body);
        }

        public async Task<(bool ok, string? error)> UpdateAsync(PantallaDto dto, string? token = null)
        {
            var req = new HttpRequestMessage(HttpMethod.Put, $"pantallas/{dto.PantallaID}");
            req.Content = JsonContent.Create(dto);
            if (token != null) req.Headers.Add("token", token);
            var resp = await _http.SendAsync(req);
            if (resp.IsSuccessStatusCode) return (true, null);
            var body = await resp.Content.ReadAsStringAsync();
            return (false, body);
        }

        public async Task<bool> DeleteAsync(Guid id, string? token = null)
        {
            var req = new HttpRequestMessage(HttpMethod.Delete, $"pantallas/{id}");
            if (token != null) req.Headers.Add("token", token);
            var resp = await _http.SendAsync(req);
            return resp.IsSuccessStatusCode;
        }
    }
}
