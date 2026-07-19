using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SitioAdministrativoCarnetDigital.Services.MicroServicioPantallas;

namespace SitioAdministrativoCarnetDigital.Pages.ModuloPantallas
{
    public class PantallasModel : PageModel
    {
        private readonly IPantallasApiClient _api;
        public PantallasModel(IPantallasApiClient api) => _api = api;

        public List<PantallaDto> Pantallas { get; set; } = new();
        public int PageNumber { get; set; } = 1;
        public int TotalPaginas { get; set; } = 1;
        public string? Busqueda { get; set; }
        public string? Mensaje { get; set; }
        public string? Error { get; set; }

        [BindProperty] public PantallaDto Form { get; set; } = new();

        private string? Token => null;

        public async Task OnGetAsync(int page = 1, string? busqueda = null)
        {
            Busqueda = busqueda;
            PageNumber = page;
            try
            {
                var result = await _api.GetAllAsync(page, 15, busqueda, Token);
                Pantallas = result.Items;
                TotalPaginas = (int)Math.Ceiling((double)result.Total / 15);
            }
            catch (Exception ex) { Error = $"Error al cargar pantallas: {ex.Message}"; }
        }

        public async Task<IActionResult> OnPostCrearAsync()
        {
            var (ok, error) = await _api.CreateAsync(Form, Token);
            if (!ok) Error = $"Error al crear: {error}";
            else Mensaje = "Pantalla creada correctamente.";
            await OnGetAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostEditarAsync()
        {
            var (ok, error) = await _api.UpdateAsync(Form, Token);
            if (!ok) Error = $"Error al actualizar: {error}";
            else Mensaje = "Pantalla actualizada correctamente.";
            await OnGetAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostEliminarAsync(Guid id)
        {
            var ok = await _api.DeleteAsync(id, Token);
            if (!ok) Error = "No se pudo eliminar la pantalla.";
            else Mensaje = "Pantalla eliminada correctamente.";
            await OnGetAsync();
            return Page();
        }
    }
}