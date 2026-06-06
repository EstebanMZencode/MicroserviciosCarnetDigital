using MicroServicioPantallas.Entities;

namespace MicroServicioPantallas.Services
{
    public interface IPantallaService
    {
        Task<IEnumerable<Pantalla>> GetAllAsync();
        Task<Pantalla?> GetByIdAsync(int id);
        Task<int> CreateAsync(Pantalla pantalla);
        Task<int> UpdateAsync(Pantalla pantalla);
        Task<int> DeleteAsync(int id);
    }
}
