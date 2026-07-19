namespace SitioAdministrativoCarnetDigital.Services.MicroServicioParametros
{
    public class ParametroDto
    {
        public string Identificador { get; set; } = string.Empty;
        public string Valor { get; set; } = string.Empty;
        public bool Estado { get; set; }
        public DateTime? FechaCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }
    }

    public class ParametroPagedResult
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int Total { get; set; }
        public List<ParametroDto> Items { get; set; } = new();
    }

    public interface IParametrosApiClient
    {
        Task<ParametroPagedResult> GetAllAsync(int page = 1, int pageSize = 15, string? search = null, string? token = null);
        Task<ParametroDto?> GetByIdAsync(string id, string? token = null);
        Task<(bool ok, string? error)> CreateAsync(ParametroDto dto, string? token = null);
        Task<(bool ok, string? error)> UpdateAsync(ParametroDto dto, string? token = null);
        Task<bool> DeleteAsync(string id, string? token = null);
    }
}
