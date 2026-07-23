using MicroServicioPantallas.Entities;

namespace MicroServicioPantallas.Services
{
    public interface IPantallaService
    {
        Task<(IEnumerable<Pantalla> Items, int Total)> GetPaginadoAsync(
            int pageNumber, int pageSize, string? searchTerm,
            string sortColumn, string sortDirection, bool incluirEliminados);

        Task<Pantalla?> GetByIdAsync(Guid id);
        Task<Pantalla?> CreateAsync(Pantalla pantalla);
        Task<int> UpdateAsync(Pantalla pantalla);
        Task<int> LogicDeleteAsync(Guid id);
    }
}