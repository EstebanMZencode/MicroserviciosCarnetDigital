namespace SitioAdministrativoCarnetDigital.Services.MicroServicioBitacoras
{
    public class BitacoraDto
    {
        public Guid BitacoraID { get; set; }
        public Guid UsuarioID { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public DateTime? FechaHora { get; set; }
    }

    public class BitacoraPagedResult
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int Total { get; set; }
        public List<BitacoraDto> Items { get; set; } = new();
    }

    public interface IBitacorasApiClient
    {
        void SetToken(string token);
        Task<BitacoraPagedResult> GetAllAsync(int page = 1, int pageSize = 15, string? search = null);
    }
}