using MicroServicioParametros.Entities;

namespace MicroServicioParametros.Services
{
    public interface IParametroService
    {
        Task<IEnumerable<Parametro>> GetAllAsync();
        Task<Parametro?> GetByIdAsync(string id);
        Task<int> CreateAsync(Parametro parametro);
        Task<int> UpdateAsync(Parametro parametro);
        Task<int> DeleteAsync(string id);
    }
}
