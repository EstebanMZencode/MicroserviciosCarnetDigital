using System.Net;
using System.Net.Http.Json;

namespace SitioAdministrativoCarnetDigital.Services.MicroServicioAreasTrabajo
{
    public class AreasTrabajoApiClient : IAreasTrabajoApiClient
    {
        private readonly HttpClient _http;

        public AreasTrabajoApiClient(HttpClient http)
        {
            _http = http;
        }

        public void SetToken(string token)
        {
            if (!_http.DefaultRequestHeaders.Contains("token"))
                _http.DefaultRequestHeaders.Add("token", token);
        }

        public async Task<List<AreaTrabajoDto>> GetAllAsync(CancellationToken ct = default)
        {
            var baseUrl = _http.BaseAddress!.ToString().TrimEnd('/');
            var result = await _http.GetFromJsonAsync<List<AreaTrabajoDto>>($"{baseUrl}/api/area/", ct);
            return result ?? new List<AreaTrabajoDto>();
        }

        public async Task<AreaTrabajoDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            var baseUrl = _http.BaseAddress!.ToString().TrimEnd('/');
            return await _http.GetFromJsonAsync<AreaTrabajoDto>($"{baseUrl}/api/area/{id}", ct);
        }

        public async Task<(bool ok, HttpStatusCode status, string? message)> CreateAsync(AreaTrabajoDto area, CancellationToken ct = default)
        {
            var baseUrl = _http.BaseAddress!.ToString().TrimEnd('/');
            var response = await _http.PostAsJsonAsync($"{baseUrl}/api/area/", area, ct);
            string? msg = null;
            try
            {
                var payload = await response.Content.ReadFromJsonAsync<Dictionary<string, string?>>(cancellationToken: ct);
                if (payload != null && payload.TryGetValue("message", out var m))
                    msg = m;
            }
            catch { }
            return (response.IsSuccessStatusCode, response.StatusCode, msg);
        }

        public async Task<bool> UpdateAsync(AreaTrabajoDto area, CancellationToken ct = default)
        {
            var baseUrl = _http.BaseAddress!.ToString().TrimEnd('/');
            var response = await _http.PutAsJsonAsync($"{baseUrl}/api/area/{area.AreaTrabID}", area, ct);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var baseUrl = _http.BaseAddress!.ToString().TrimEnd('/');
            var response = await _http.DeleteAsync($"{baseUrl}/api/area/{id}", ct);
            return response.IsSuccessStatusCode;
        }
    }
}