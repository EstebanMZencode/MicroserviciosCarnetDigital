using System.Net.Http.Json;

namespace SitioAdministrativoCarnetDigital.Services.MicroServicioParametros
{
    public class ParametrosApiClient : IParametrosApiClient
    {
        private readonly HttpClient _http;
        public ParametrosApiClient(HttpClient http) => _http = http;

        public async Task<ParametroPagedResult> GetAllAsync(int page = 1, int pageSize = 15, string? search = null, string? token = null)
        {
            var req = new HttpRequestMessage(HttpMethod.Get,
                $"parametro?pageNumber={page}&pageSize={pageSize}" +
                (string.IsNullOrWhiteSpace(search) ? "" : $"&searchTerm={Uri.EscapeDataString(search)}"));
            if (token != null) req.Headers.Add("token", token);
            var resp = await _http.SendAsync(req);
            resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadFromJsonAsync<ParametroPagedResult>() ?? new();
        }

        public async Task<ParametroDto?> GetByIdAsync(string id, string? token = null)
        {
            var req = new HttpRequestMessage(HttpMethod.Get, $"parametro/{id}");
            if (token != null) req.Headers.Add("token", token);
            var resp = await _http.SendAsync(req);
            if (!resp.IsSuccessStatusCode) return null;
            return await resp.Content.ReadFromJsonAsync<ParametroDto>();
        }

        public async Task<(bool ok, string? error)> CreateAsync(ParametroDto dto, string? token = null)
        {
            var req = new HttpRequestMessage(HttpMethod.Post, "parametro");
            req.Content = JsonContent.Create(dto);
            if (token != null) req.Headers.Add("token", token);
            var resp = await _http.SendAsync(req);
            if (resp.IsSuccessStatusCode) return (true, null);
            var body = await resp.Content.ReadAsStringAsync();
            return (false, body);
        }

        public async Task<(bool ok, string? error)> UpdateAsync(ParametroDto dto, string? token = null)
        {
            var req = new HttpRequestMessage(HttpMethod.Put, $"parametro/{dto.Identificador}");
            req.Content = JsonContent.Create(dto);
            if (token != null) req.Headers.Add("token", token);
            var resp = await _http.SendAsync(req);
            if (resp.IsSuccessStatusCode) return (true, null);
            var body = await resp.Content.ReadAsStringAsync();
            return (false, body);
        }

        public async Task<bool> DeleteAsync(string id, string? token = null)
        {
            var req = new HttpRequestMessage(HttpMethod.Delete, $"parametro/{id}");
            if (token != null) req.Headers.Add("token", token);
            var resp = await _http.SendAsync(req);
            return resp.IsSuccessStatusCode;
        }
    }
}
