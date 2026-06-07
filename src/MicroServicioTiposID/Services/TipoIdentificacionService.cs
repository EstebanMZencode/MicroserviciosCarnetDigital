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

    public async Task<TipoIdentificacion?> GetByIdAsync(int id)
        => await _repository.GetByIdAsync(id);

    public async Task<(bool Success, string Message, int Id)> CreateAsync(TipoIdentificacion tipo)
    {
        if (string.IsNullOrWhiteSpace(tipo.Nombre))
            return (false, "El nombre es requerido.", 0);

        var id = await _repository.CreateAsync(tipo);
        return (true, "Tipo de identificación creado exitosamente.", id);
    }

    public async Task<(bool Success, string Message)> UpdateAsync(int id, TipoIdentificacion tipo)
    {
        if (string.IsNullOrWhiteSpace(tipo.Nombre))
            return (false, "El nombre es requerido.");

        var existing = await _repository.GetByIdAsync(id);
        if (existing is null)
            return (false, "Tipo de identificación no encontrado.");

        tipo.Id = id;
        await _repository.UpdateAsync(tipo);
        return (true, "Tipo de identificación actualizado exitosamente.");
    }

    public async Task<(bool Success, string Message)> DeleteAsync(int id)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing is null)
            return (false, "Tipo de identificación no encontrado.");

        await _repository.DeleteAsync(id);
        return (true, "Tipo de identificación eliminado exitosamente.");
    }
}