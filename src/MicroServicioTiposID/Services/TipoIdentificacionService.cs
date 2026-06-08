using MicroServicioTiposID.Entities;
using MicroServicioTiposID.Repository;

namespace MicroServicioTiposID.Services;

public class TipoIdentificacionService : ITipoIdentificacionService
{
    private readonly TipoIdentificacionRepository _repository;

    public TipoIdentificacionService(TipoIdentificacionRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<TipoIdentificacion>> GetAllAsync()
        => await _repository.GetAllAsync();

    public async Task<TipoIdentificacion?> GetByIdAsync(Guid id)
        => await _repository.GetByIdAsync(id);

    public async Task<(bool Success, string Message, Guid Id)> CreateAsync(TipoIdentificacionRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.NombreTipoIdent))
            return (false, "El nombre del tipo de identificación es requerido.", Guid.Empty);

        var id = await _repository.CreateAsync(request);
        return (true, "Tipo de identificación creado exitosamente.", id);
    }

    public async Task<(bool Success, string Message)> UpdateAsync(Guid id, TipoIdentificacionRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.NombreTipoIdent))
            return (false, "El nombre del tipo de identificación es requerido.");

        var existing = await _repository.GetByIdAsync(id);
        if (existing is null)
            return (false, "Tipo de identificación no encontrado.");

        await _repository.UpdateAsync(id, request);
        return (true, "Tipo de identificación actualizado exitosamente.");
    }

    public async Task<(bool Success, string Message)> DeleteAsync(Guid id)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing is null)
            return (false, "Tipo de identificación no encontrado.");

        await _repository.DeleteAsync(id);
        return (true, "Tipo de identificación eliminado exitosamente.");
    }
}