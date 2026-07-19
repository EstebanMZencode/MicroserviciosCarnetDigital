using System.Net.Http.Json;

namespace SitioAdministrativoCarnetDigital.Services.MicroServicioBitacoras
{
    public class BitacorasApiClient : IBitacorasApiClient
    {
        private readonly HttpClient _http;
        public BitacorasApiClient(HttpClient http) => _http = http;

        public async Task<BitacoraPagedResult> GetAllAsync(int page = 1, int pageSize = 15, string? search = null, string? token = null)
        {
            var req = new HttpRequestMessage(HttpMethod.Get,
                $"bitacora?pageNumber={page}&pageSize={pageSize}" +
                (string.IsNullOrWhiteSpace(search) ? "" : $"&searchTerm={Uri.EscapeDataString(search)}"));
            if (token != null) req.Headers.Add("token", token);
            var resp = await _http.SendAsync(req);
            resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadFromJsonAsync<BitacoraPagedResult>() ?? new();
        }
    }
}
