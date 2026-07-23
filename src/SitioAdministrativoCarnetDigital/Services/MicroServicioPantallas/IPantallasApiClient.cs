using System.Net;

namespace SitioAdministrativoCarnetDigital.Services.MicroServicioPantallas
{
    public class PantallaDto
    {
        public Guid PantallaID { get; set; }
        public string NombrePantalla { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Ruta { get; set; } = string.Empty;
        public bool Estado { get; set; }
        public DateTime? FechaCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }
    }

    public class PantallaPagedResult
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int Total { get; set; }
        public List<PantallaDto> Items { get; set; } = new();
    }

    public interface IPantallasApiClient
    {
        Task<PantallaPagedResult> GetAllAsync(int page = 1, int pageSize = 15, string? search = null, string? token = null);
        Task<PantallaDto?> GetByIdAsync(Guid id, string? token = null);
        Task<(bool ok, string? error)> CreateAsync(PantallaDto dto, string? token = null);
        Task<(bool ok, string? error)> UpdateAsync(PantallaDto dto, string? token = null);
        Task<bool> DeleteAsync(Guid id, string? token = null);
    }
}
