using System.Net;

namespace SitioAdministrativoCarnetDigital.Services.MicroServicioAreasTrabajo
{
    public interface IAreasTrabajoApiClient
    {
        Task<List<AreaTrabajoDto>> GetAllAsync(CancellationToken ct = default);
        Task<AreaTrabajoDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<(bool ok, HttpStatusCode status, string? message)> CreateAsync(AreaTrabajoDto area, CancellationToken ct = default);
        Task<bool> UpdateAsync(AreaTrabajoDto area, CancellationToken ct = default);
        Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
    }
}