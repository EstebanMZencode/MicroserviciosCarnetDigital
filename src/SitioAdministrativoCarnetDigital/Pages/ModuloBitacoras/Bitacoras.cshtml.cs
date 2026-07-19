using Microsoft.AspNetCore.Mvc.RazorPages;
using SitioAdministrativoCarnetDigital.Services.MicroServicioBitacoras;

namespace SitioAdministrativoCarnetDigital.Pages.ModuloBitacoras
{
    public class BitacorasModel : PageModel
    {
        private readonly IBitacorasApiClient _api;
        public BitacorasModel(IBitacorasApiClient api) => _api = api;

        public List<BitacoraDto> Bitacoras { get; set; } = new();
        public int PageNumber { get; set; } = 1;
        public int TotalPaginas { get; set; } = 1;
        public string? Busqueda { get; set; }
        public string? Error { get; set; }

        private string? Token => null;

        public async Task OnGetAsync(int page = 1, string? busqueda = null)
        {
            Busqueda = busqueda;
            PageNumber = page;
            try
            {
                var result = await _api.GetAllAsync(page, 15, busqueda, Token);
                Bitacoras = result.Items;
                TotalPaginas = (int)Math.Ceiling((double)result.Total / 15);
            }
            catch (Exception ex) { Error = $"Error al cargar bitácoras: {ex.Message}"; }
        }
    }
}