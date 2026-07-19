using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SitioAdministrativoCarnetDigital.Services.MicroServicioParametros;

namespace SitioAdministrativoCarnetDigital.Pages.ModuloParametros
{
    public class ParametrosModel : PageModel
    {
        private readonly IParametrosApiClient _api;
        public ParametrosModel(IParametrosApiClient api) => _api = api;

        public List<ParametroDto> Parametros { get; set; } = new();
        public int PageNumber { get; set; } = 1;
        public int TotalPaginas { get; set; } = 1;
        public string? Busqueda { get; set; }
        public string? Mensaje { get; set; }
        public string? Error { get; set; }

        [BindProperty] public ParametroDto Form { get; set; } = new();
        [BindProperty] public bool EsEdicion { get; set; }

        private string? Token => null;

        public async Task OnGetAsync(int page = 1, string? busqueda = null)
        {
            Busqueda = busqueda;
            PageNumber = page;
            try
            {
                var result = await _api.GetAllAsync(page, 15, busqueda, Token);
                Parametros = result.Items;
                TotalPaginas = (int)Math.Ceiling((double)result.Total / 15);
            }
            catch (Exception ex) { Error = $"Error al cargar parámetros: {ex.Message}"; }
        }

        public async Task<IActionResult> OnPostCrearAsync()
        {
            var (ok, error) = await _api.CreateAsync(Form, Token);
            if (!ok) Error = $"Error al crear: {error}";
            else Mensaje = "Parámetro creado correctamente.";
            await OnGetAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostEditarAsync()
        {
            var (ok, error) = await _api.UpdateAsync(Form, Token);
            if (!ok) Error = $"Error al actualizar: {error}";
            else Mensaje = "Parámetro actualizado correctamente.";
            await OnGetAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostEliminarAsync(string id)
        {
            var ok = await _api.DeleteAsync(id, Token);
            if (!ok) Error = "No se pudo eliminar el parámetro.";
            else Mensaje = "Parámetro eliminado correctamente.";
            await OnGetAsync();
            return Page();
        }
    }
}