using MicroServicioInstituciones.Entities;

namespace MicroServicioInstituciones.Services;

public interface IInstitucionService
{
    Task<IEnumerable<Institucion>> GetAllAsync();
    Task<Institucion?> GetByIdAsync(Guid id);
    Task<(bool Success, string Message, Guid Id)> CreateAsync(InstitucionRequest request);
    Task<(bool Success, string Message)> UpdateAsync(Guid id, InstitucionRequest request);
    Task<(bool Success, string Message)> DeleteAsync(Guid id);
}