using MicroServicioParametros.Entities;

namespace MicroServicioParametros.Services
{
    public interface IParametroService
    {
        Task<(IEnumerable<Parametro> Items, int Total)> GetPaginadoAsync(
            int pageNumber, int pageSize, string? searchTerm,
            string sortColumn, string sortDirection, bool incluirEliminados);

        Task<Parametro?> GetByIdAsync(string id);
        Task<Parametro?> CreateAsync(Parametro parametro, Guid usuarioId, string? token = null);
        Task<int> UpdateAsync(Parametro parametro, Guid usuarioId, string? token = null);
        Task<int> LogicDeleteAsync(string id, Guid usuarioId, string? token = null);
    }
}