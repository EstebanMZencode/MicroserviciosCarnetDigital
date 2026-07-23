using MicroServicioParametros.Entities;

namespace MicroServicioParametros.Services
{
    public interface IParametroService
    {
        Task<(IEnumerable<Parametro> Items, int Total)> GetPaginadoAsync(
            int pageNumber, int pageSize, string? searchTerm,
            string sortColumn, string sortDirection, bool incluirEliminados);

        Task<Parametro?> GetByIdAsync(string id);
        Task<Parametro?> CreateAsync(Parametro parametro);
        Task<int> UpdateAsync(Parametro parametro);
        Task<int> LogicDeleteAsync(string id);
    }
}
