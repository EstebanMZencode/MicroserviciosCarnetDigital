using MicroServicioBitacoras.Entities;

namespace MicroServicioBitacoras.Services
{
    public interface IBitacoraService
    {
        Task<IEnumerable<Bitacora>> GetAllAsync();
        Task<int> CreateAsync(Bitacora bitacora);
    }
}
