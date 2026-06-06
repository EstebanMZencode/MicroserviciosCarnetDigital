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

        public async Task<IEnumerable<Parametro>> GetAllAsync()
        {
            return await _parametroRepository.GetAllAsync();
        }

        public async Task<Parametro?> GetByIdAsync(string id)
        {
            return await _parametroRepository.GetByIdAsync(id);
        }

        public async Task<int> CreateAsync(Parametro parametro)
        {
            return await _parametroRepository.CreateAsync(parametro);
        }

        public async Task<int> UpdateAsync(Parametro parametro)
        {
            return await _parametroRepository.UpdateAsync(parametro);
        }

        public async Task<int> DeleteAsync(string id)
        {
            return await _parametroRepository.DeleteAsync(id);
        }
    }
}
