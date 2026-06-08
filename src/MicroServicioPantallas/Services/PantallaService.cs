using MicroServicioPantallas.Entities;
using MicroServicioPantallas.Repository;

namespace MicroServicioPantallas.Services
{
    public class PantallaService : IPantallaService
    {
        private readonly PantallaRepository _pantallaRepository;

        public PantallaService(PantallaRepository pantallaRepository)
        {
            _pantallaRepository = pantallaRepository;
        }

        public async Task<(IEnumerable<Pantalla> Items, int Total)> GetPaginadoAsync(
            int pageNumber, int pageSize, string? searchTerm,
            string sortColumn, string sortDirection, bool incluirEliminados)
        {
            return await _pantallaRepository.GetPaginadoAsync(
                pageNumber, pageSize, searchTerm, sortColumn, sortDirection, incluirEliminados);
        }

        public async Task<Pantalla?> GetByIdAsync(Guid id) => await _pantallaRepository.GetByIdAsync(id);
        public async Task<Pantalla?> CreateAsync(Pantalla pantalla) => await _pantallaRepository.CreateAsync(pantalla);
        public async Task<int> UpdateAsync(Pantalla pantalla) => await _pantallaRepository.UpdateAsync(pantalla);
        public async Task<int> LogicDeleteAsync(Guid id) => await _pantallaRepository.LogicDeleteAsync(id);
    }
}