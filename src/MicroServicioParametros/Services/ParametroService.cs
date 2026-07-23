using MicroServicioParametros.Entities;
using MicroServicioParametros.Repository;
using System.Text;
using System.Text.Json;

namespace MicroServicioParametros.Services
{
    public class ParametroService : IParametroService
    {
        private readonly ParametroRepository _parametroRepository;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;

        public ParametroService(ParametroRepository parametroRepository, IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _parametroRepository = parametroRepository;
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
                Console.WriteLine($"Bitacora response: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error bitacora: {ex.Message}");
            }
        }

        public async Task<(IEnumerable<Parametro> Items, int Total)> GetPaginadoAsync(
            int pageNumber, int pageSize, string? searchTerm,
            string sortColumn, string sortDirection, bool incluirEliminados)
        {
            return await _parametroRepository.GetPaginadoAsync(
                pageNumber, pageSize, searchTerm, sortColumn, sortDirection, incluirEliminados);
        }

        public async Task<Parametro?> GetByIdAsync(string id)
        {
            return await _parametroRepository.GetByIdAsync(id);
        }

        public async Task<Parametro?> CreateAsync(Parametro parametro, Guid usuarioId, string? token = null)
        {
            var result = await _parametroRepository.CreateAsync(parametro);
            if (result != null)
                await RegistrarBitacoraAsync(usuarioId, $"El usuario crea parámetro: {JsonSerializer.Serialize(result)}", token);
            return result;
        }

        public async Task<int> UpdateAsync(Parametro parametro, Guid usuarioId, string? token = null)
        {
            var anterior = await _parametroRepository.GetByIdAsync(parametro.Identificador);
            var result = await _parametroRepository.UpdateAsync(parametro);
            await RegistrarBitacoraAsync(usuarioId, $"El usuario actualiza parámetro. Anterior: {JsonSerializer.Serialize(anterior)}, Actual: {JsonSerializer.Serialize(parametro)}", token);
            return result;
        }

        public async Task<int> LogicDeleteAsync(string id, Guid usuarioId, string? token = null)
        {
            var anterior = await _parametroRepository.GetByIdAsync(id);
            var result = await _parametroRepository.LogicDeleteAsync(id);
            await RegistrarBitacoraAsync(usuarioId, $"El usuario elimina parámetro: {JsonSerializer.Serialize(anterior)}", token);
            return result;
        }
    }
}