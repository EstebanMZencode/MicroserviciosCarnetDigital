using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SitioAdministrativoCarnetDigital.Services.MicroServicioPantallas;

namespace SitioAdministrativoCarnetDigital.Pages.ModuloPantallas
{
    public class PantallaInput
    {
        public Guid? PantallaID { get; set; }
        public string NombrePantalla { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Ruta { get; set; } = string.Empty;
    }

    public class PantallasModel : PageModel
    {
        private readonly IPantallasApiClient _api;
        public List<PantallaDto> Pantallas { get; set; } = new();
        public string? MensajeError { get; set; }

        public PantallasModel(IPantallasApiClient api) => _api = api;

        public async Task<IActionResult> OnGetAsync()
        {
            ViewData["Section"] = "Administración de Pantallas";
            ViewData["UserName"] = Request.Cookies["UserName"] ?? "Usuario de prueba";
            ViewData["UserRole"] = Request.Cookies["UserRole"] ?? "";
            ViewData["UserInitials"] = "U";
            try
            {
                var result = await _api.GetAllAsync(1, 15);
                Pantallas = result.Items;
            }
            catch (Exception ex) { MensajeError = $"Error al cargar: {ex.Message}"; }
            return Page();
        }

        public async Task<IActionResult> OnPostGuardarAsync([FromBody] PantallaInput input)
        {
            try
            {
                var dto = new PantallaDto
                {
                    PantallaID = input.PantallaID ?? Guid.Empty,
                    NombrePantalla = input.NombrePantalla,
                    Descripcion = input.Descripcion,
                    Ruta = input.Ruta,
                    Estado = true
                };

                if (input.PantallaID == null || input.PantallaID == Guid.Empty)
                {
                    var (ok, error) = await _api.CreateAsync(dto);
                    if (ok) return new JsonResult(new { exito = true, mensaje = "Pantalla creada correctamente." });
                    return new JsonResult(new { exito = false, mensaje = error }) { StatusCode = 400 };
                }
                else
                {
                    var (ok, error) = await _api.UpdateAsync(dto);
                    if (ok) return new JsonResult(new { exito = true, mensaje = "Pantalla actualizada correctamente." });
                    return new JsonResult(new { exito = false, mensaje = error }) { StatusCode = 400 };
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new { exito = false, mensaje = ex.Message }) { StatusCode = 400 };
            }
        }

        public async Task<IActionResult> OnPostEliminarAsync([FromBody] Guid id)
        {
            try
            {
                var ok = await _api.DeleteAsync(id);
                if (ok) return new JsonResult(new { exito = true, mensaje = "Pantalla eliminada correctamente." });
                return new JsonResult(new { exito = false, mensaje = "No se pudo eliminar." }) { StatusCode = 400 };
            }
            catch (Exception ex)
            {
                return new JsonResult(new { exito = false, mensaje = ex.Message }) { StatusCode = 400 };
            }
        }
    }
}