using MicroServicioParametros.Entities;
using MicroServicioParametros.Repository;

namespace MicroServicioParametros.Services
{
    public class ParametroService : IParametroService
    {
        private readonly ParametroRepository _parametroRepository;

        public ParametroService(ParametroRepository parametroRepository)
        {
            _parametroRepository = parametroRepository;
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

        public async Task<Parametro?> CreateAsync(Parametro parametro)
        {
            return await _parametroRepository.CreateAsync(parametro);
        }

        public async Task<int> UpdateAsync(Parametro parametro)
        {
            return await _parametroRepository.UpdateAsync(parametro);
        }

        public async Task<int> LogicDeleteAsync(string id)
        {
            return await _parametroRepository.LogicDeleteAsync(id);
        }
    }
}