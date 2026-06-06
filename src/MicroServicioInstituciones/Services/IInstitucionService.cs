using MicroServicioInstituciones.Entities;

namespace MicroServicioInstituciones.Services;

public interface IInstitucionService
{
    Task<IEnumerable<Institucion>> GetAllAsync();
    Task<Institucion?> GetByIdAsync(int id);
    Task<(bool Success, string Message, int Id)> CreateAsync(InstitucionRequest request);
    Task<(bool Success, string Message)> UpdateAsync(int id, InstitucionRequest request);
    Task<(bool Success, string Message)> DeleteAsync(int id);
}