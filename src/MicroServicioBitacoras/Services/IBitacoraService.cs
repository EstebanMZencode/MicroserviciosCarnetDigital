using MicroServicioBitacoras.Entities;

namespace MicroServicioBitacoras.Services
{
    public interface IBitacoraService
    {
        Task<(IEnumerable<Bitacora> Items, int Total)> GetPaginadoAsync(
            int pageNumber, int pageSize, string? searchTerm,
            string sortColumn, string sortDirection, bool incluirEliminados);

        Task<Bitacora?> CreateAsync(Bitacora bitacora);
    }
}