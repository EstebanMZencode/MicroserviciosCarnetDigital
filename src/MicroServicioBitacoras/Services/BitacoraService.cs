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

        public async Task<IEnumerable<Bitacora>> GetAllAsync()
        {
            return await _bitacoraRepository.GetAllAsync();
        }

        public async Task<int> CreateAsync(Bitacora bitacora)
        {
            return await _bitacoraRepository.CreateAsync(bitacora);
        }
    }
}
