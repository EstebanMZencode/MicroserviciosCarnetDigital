using MicroServicioPantallas.Entities;

namespace MicroServicioPantallas.Services
{
    public interface IPantallaService
    {
        Task<(IEnumerable<Pantalla> Items, int Total)> GetPaginadoAsync(
            int pageNumber, int pageSize, string? searchTerm,
            string sortColumn, string sortDirection, bool incluirEliminados);

        Task<Pantalla?> GetByIdAsync(Guid id);
        Task<Pantalla?> CreateAsync(Pantalla pantalla, Guid usuarioId, string? token = null);
        Task<int> UpdateAsync(Pantalla pantalla, Guid usuarioId, string? token = null);
        Task<int> LogicDeleteAsync(Guid id, Guid usuarioId, string? token = null);
    }
}