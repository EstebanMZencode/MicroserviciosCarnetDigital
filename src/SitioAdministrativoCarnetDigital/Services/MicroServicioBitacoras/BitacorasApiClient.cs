using System.Net.Http.Json;

namespace SitioAdministrativoCarnetDigital.Services.MicroServicioBitacoras
{
    public class BitacorasApiClient : IBitacorasApiClient
    {
        private readonly HttpClient _http;

        public BitacorasApiClient(HttpClient http)
        {
            _http = http;
        }

        public void SetToken(string token)
        {
            if (!_http.DefaultRequestHeaders.Contains("token"))
                _http.DefaultRequestHeaders.Add("token", token);
        }

        public async Task<BitacoraPagedResult> GetAllAsync(int page = 1, int pageSize = 15, string? search = null)
        {
            var baseUrl = _http.BaseAddress!.ToString().TrimEnd('/');
            var url = $"{baseUrl}/bitacora?pageNumber={page}&pageSize={pageSize}";
            if (!string.IsNullOrWhiteSpace(search))
                url += $"&searchTerm={Uri.EscapeDataString(search)}";
            var resp = await _http.GetAsync(url);
            resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadFromJsonAsync<BitacoraPagedResult>() ?? new();
        }
    }
}