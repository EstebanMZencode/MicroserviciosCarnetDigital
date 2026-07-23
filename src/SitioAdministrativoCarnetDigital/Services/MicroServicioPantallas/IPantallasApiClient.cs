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
        void SetToken(string token);
        Task<PantallaPagedResult> GetAllAsync(int page = 1, int pageSize = 15, string? search = null);
        Task<PantallaDto?> GetByIdAsync(Guid id);
        Task<(bool ok, string? error)> CreateAsync(PantallaDto dto);
        Task<(bool ok, string? error)> UpdateAsync(PantallaDto dto);
        Task<bool> DeleteAsync(Guid id);
    }
}