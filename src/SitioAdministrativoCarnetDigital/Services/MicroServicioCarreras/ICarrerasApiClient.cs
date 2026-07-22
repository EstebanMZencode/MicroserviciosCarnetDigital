using System.Net;

namespace SitioAdministrativoCarnetDigital.Services.MicroServicioCarreras
{
    public interface ICarrerasApiClient
    {
        Task<List<CarreraDto>> GetAllAsync(CancellationToken ct = default);
        Task<CarreraDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<(bool ok, HttpStatusCode status, string? message)> CreateAsync(CarreraDto carrera, CancellationToken ct = default);
        Task<bool> UpdateAsync(CarreraDto carrera, CancellationToken ct = default);
        Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
    }
} 