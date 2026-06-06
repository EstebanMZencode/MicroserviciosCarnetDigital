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

        public async Task<IEnumerable<Pantalla>> GetAllAsync()
        {
            return await _pantallaRepository.GetAllAsync();
        }

        public async Task<Pantalla?> GetByIdAsync(int id)
        {
            return await _pantallaRepository.GetByIdAsync(id);
        }

        public async Task<int> CreateAsync(Pantalla pantalla)
        {
            return await _pantallaRepository.CreateAsync(pantalla);
        }

        public async Task<int> UpdateAsync(Pantalla pantalla)
        {
            return await _pantallaRepository.UpdateAsync(pantalla);
        }

        public async Task<int> DeleteAsync(int id)
        {
            return await _pantallaRepository.DeleteAsync(id);
        }
    }
}
