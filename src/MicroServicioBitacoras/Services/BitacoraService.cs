using MicroServicioBitacoras.Entities;
using MicroServicioBitacoras.Repository;

namespace MicroServicioBitacoras.Services
{
    public class BitacoraService : IBitacoraService
    {
        private readonly BitacoraRepository _bitacoraRepository;

        public BitacoraService(BitacoraRepository bitacoraRepository)
        {
            _bitacoraRepository = bitacoraRepository;
        }

        public async Task<(IEnumerable<Bitacora> Items, int Total)> GetPaginadoAsync(
            int pageNumber, int pageSize, string? searchTerm,
            string sortColumn, string sortDirection, bool incluirEliminados)
        {
            return await _bitacoraRepository.GetPaginadoAsync(
                pageNumber, pageSize, searchTerm, sortColumn, sortDirection, incluirEliminados);
        }

        public async Task<Bitacora?> CreateAsync(Bitacora bitacora)
        {
            return await _bitacoraRepository.CreateAsync(bitacora);
        }
    }
}