using System.Net;

namespace SitioAdministrativoCarnetDigital.Services.MicroServicioInstituciones
{
    public interface IInstitucionesApiClient
    {
        void SetToken(string token);
        Task<List<InstitucionDto>> GetAllAsync(CancellationToken ct = default);
        Task<InstitucionDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<(bool ok, HttpStatusCode status, string? message)> CreateAsync(InstitucionDto institucion, CancellationToken ct = default);
        Task<bool> UpdateAsync(InstitucionDto institucion, CancellationToken ct = default);
        Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
    }
}