using MicroServicioTiposID.Entities;

namespace MicroServicioTiposID.Services;

public interface ITipoIdentificacionService
{
    Task<IEnumerable<TipoIdentificacion>> GetAllAsync();
    Task<TipoIdentificacion?> GetByIdAsync(int id);
    Task<(bool Success, string Message, int Id)> CreateAsync(TipoIdentificacion tipo);
    Task<(bool Success, string Message)> UpdateAsync(int id, TipoIdentificacion tipo);
    Task<(bool Success, string Message)> DeleteAsync(int id);
}