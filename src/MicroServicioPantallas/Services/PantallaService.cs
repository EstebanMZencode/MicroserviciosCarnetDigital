using MicroServicioPantallas.Entities;
using MicroServicioPantallas.Repository;
using System.Text;
using System.Text.Json;

namespace MicroServicioPantallas.Services
{
    public class PantallaService : IPantallaService
    {
        private readonly PantallaRepository _pantallaRepository;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;

        public PantallaService(PantallaRepository pantallaRepository, IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _pantallaRepository = pantallaRepository;
            _httpClientFactory = httpClientFactory;
            _config = config;
        }

        private async Task RegistrarBitacoraAsync(Guid usuarioId, string descripcion, string? token = null)
        {
            try
            {
                var bitacoraUrl = _config["MicroServicios:BitacoraUrl"];
                var client = _httpClientFactory.CreateClient();
                if (!string.IsNullOrEmpty(token))
                    client.DefaultRequestHeaders.Add("token", token);
                var body = new { UsuarioID = usuarioId, Descripcion = descripcion };
                var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
                var response = await client.PostAsync($"{bitacoraUrl}/bitacora", content);
                var responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Bitacora response: {response.StatusCode} - {responseBody}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error bitacora: {ex.Message}");
            }
        }

        public async Task<(IEnumerable<Pantalla> Items, int Total)> GetPaginadoAsync(
            int pageNumber, int pageSize, string? searchTerm,
            string sortColumn, string sortDirection, bool incluirEliminados)
        {
            return await _pantallaRepository.GetPaginadoAsync(
                pageNumber, pageSize, searchTerm, sortColumn, sortDirection, incluirEliminados);
        }

        public async Task<Pantalla?> GetByIdAsync(Guid id) => await _pantallaRepository.GetByIdAsync(id);

        public async Task<Pantalla?> CreateAsync(Pantalla pantalla, Guid usuarioId, string? token = null)
        {
            var result = await _pantallaRepository.CreateAsync(pantalla);
            if (result != null)
                await RegistrarBitacoraAsync(usuarioId, $"El usuario crea pantalla: {JsonSerializer.Serialize(result)}", token);
            return result;
        }

        public async Task<int> UpdateAsync(Pantalla pantalla, Guid usuarioId, string? token = null)
        {
            var anterior = await _pantallaRepository.GetByIdAsync(pantalla.PantallaID);
            var result = await _pantallaRepository.UpdateAsync(pantalla);
            await RegistrarBitacoraAsync(usuarioId, $"El usuario actualiza pantalla. Anterior: {JsonSerializer.Serialize(anterior)}, Actual: {JsonSerializer.Serialize(pantalla)}", token);
            return result;
        }

        public async Task<int> LogicDeleteAsync(Guid id, Guid usuarioId, string? token = null)
        {
            var anterior = await _pantallaRepository.GetByIdAsync(id);
            var result = await _pantallaRepository.LogicDeleteAsync(id);
            await RegistrarBitacoraAsync(usuarioId, $"El usuario elimina pantalla: {JsonSerializer.Serialize(anterior)}", token);
            return result;
        }
    }
}