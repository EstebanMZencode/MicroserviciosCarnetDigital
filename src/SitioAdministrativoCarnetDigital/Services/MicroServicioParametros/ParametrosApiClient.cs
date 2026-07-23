using System.Net.Http.Json;

namespace SitioAdministrativoCarnetDigital.Services.MicroServicioParametros
{
    public class ParametrosApiClient : IParametrosApiClient
    {
        private readonly HttpClient _http;

        public ParametrosApiClient(HttpClient http)
        {
            _http = http;
        }

        public void SetToken(string token)
        {
            if (!_http.DefaultRequestHeaders.Contains("token"))
                _http.DefaultRequestHeaders.Add("token", token);
        }

        public async Task<ParametroPagedResult> GetAllAsync(int page = 1, int pageSize = 15, string? search = null)
        {
            var baseUrl = _http.BaseAddress!.ToString().TrimEnd('/');
            var url = $"{baseUrl}/parametro?pageNumber={page}&pageSize={pageSize}";
            if (!string.IsNullOrWhiteSpace(search))
                url += $"&searchTerm={Uri.EscapeDataString(search)}";
            var resp = await _http.GetAsync(url);
            resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadFromJsonAsync<ParametroPagedResult>() ?? new();
        }

        public async Task<ParametroDto?> GetByIdAsync(string id)
        {
            var baseUrl = _http.BaseAddress!.ToString().TrimEnd('/');
            var resp = await _http.GetAsync($"{baseUrl}/parametro/{id}");
            if (!resp.IsSuccessStatusCode) return null;
            return await resp.Content.ReadFromJsonAsync<ParametroDto>();
        }

        public async Task<(bool ok, string? error)> CreateAsync(ParametroDto dto)
        {
            var baseUrl = _http.BaseAddress!.ToString().TrimEnd('/');
            var resp = await _http.PostAsJsonAsync($"{baseUrl}/parametro", dto);
            if (resp.IsSuccessStatusCode) return (true, null);
            var body = await resp.Content.ReadAsStringAsync();
            return (false, body);
        }

        public async Task<(bool ok, string? error)> UpdateAsync(ParametroDto dto)
        {
            var baseUrl = _http.BaseAddress!.ToString().TrimEnd('/');
            var resp = await _http.PutAsJsonAsync($"{baseUrl}/parametro/{dto.Identificador}", dto);
            if (resp.IsSuccessStatusCode) return (true, null);
            var body = await resp.Content.ReadAsStringAsync();
            return (false, body);
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var baseUrl = _http.BaseAddress!.ToString().TrimEnd('/');
            var resp = await _http.DeleteAsync($"{baseUrl}/parametro/{id}");
            return resp.IsSuccessStatusCode;
        }
    }
}