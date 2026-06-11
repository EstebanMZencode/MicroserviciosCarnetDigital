using MicroServicioTiposID.Entities;

namespace MicroServicioTiposID.Services;

public interface ITipoIdentificacionService
{
    Task<IEnumerable<TipoIdentificacion>> GetAllAsync();
    Task<TipoIdentificacion?> GetByIdAsync(Guid id);
    Task<(bool Success, string Message, Guid Id)> CreateAsync(TipoIdentificacionRequest request);
    Task<(bool Success, string Message)> UpdateAsync(Guid id, TipoIdentificacionRequest request);
    Task<(bool Success, string Message)> DeleteAsync(Guid id);
}