using System.Text.RegularExpressions;
using MicroServicioInstituciones.Entities;
using MicroServicioInstituciones.Repository;

namespace MicroServicioInstituciones.Services;

public class InstitucionService : IInstitucionService
{
    private readonly InstitucionRepository _repository;

    public InstitucionService(InstitucionRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<Institucion>> GetAllAsync()
        => await _repository.GetAllAsync();

    public async Task<Institucion?> GetByIdAsync(Guid id)
        => await _repository.GetByIdAsync(id);

    public async Task<(bool Success, string Message, Guid Id)> CreateAsync(InstitucionRequest request)
    {
        var validation = Validate(request);
        if (!validation.IsValid)
            return (false, validation.Error, Guid.Empty);

        var id = await _repository.CreateAsync(request);
        return (true, "Institución creada exitosamente.", id);
    }

    public async Task<(bool Success, string Message)> UpdateAsync(Guid id, InstitucionRequest request)
    {
        var validation = Validate(request);
        if (!validation.IsValid)
            return (false, validation.Error);

        var existing = await _repository.GetByIdAsync(id);
        if (existing is null)
            return (false, "Institución no encontrada.");

        var updated = await _repository.UpdateAsync(id, request);
        return updated
            ? (true, "Institución actualizada exitosamente.")
            : (false, "No se pudo actualizar la institución.");
    }

    public async Task<(bool Success, string Message)> DeleteAsync(Guid id)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing is null)
            return (false, "Institución no encontrada.");

        await _repository.DeleteAsync(id);
        return (true, "Institución eliminada exitosamente.");
    }

    private static (bool IsValid, string Error) Validate(InstitucionRequest r)
    {
        if (string.IsNullOrWhiteSpace(r.NombreInstitucion))
            return (false, "El nombre de la institución es requerido.");

        if (string.IsNullOrWhiteSpace(r.Email))
            return (false, "El email es requerido.");

        if (!Regex.IsMatch(r.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            return (false, "El formato del email no es válido.");

        if (string.IsNullOrWhiteSpace(r.Telefono))
            return (false, "El teléfono es requerido.");

        if (!Regex.IsMatch(r.Telefono, @"^\d+$"))
            return (false, "El teléfono solo permite valores numéricos.");

        if (r.Dominios is null || r.Dominios.Count == 0 || r.Dominios.Any(string.IsNullOrWhiteSpace))
            return (false, "Debe indicar al menos un dominio válido.");

        return (true, string.Empty);
    }
}