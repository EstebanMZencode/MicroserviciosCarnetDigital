using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SitioAdministrativoCarnetDigital.Services.MicroServicioParametros;

namespace SitioAdministrativoCarnetDigital.Pages.ModuloParametros
{
    public class ParametroInput
    {
        public string? Identificador { get; set; }
        public string Valor { get; set; } = string.Empty;
    }

    public class ParametrosModel : PageModel
    {
        private readonly IParametrosApiClient _api;
        public List<ParametroDto> Parametros { get; set; } = new();
        public string? MensajeError { get; set; }

        public ParametrosModel(IParametrosApiClient api) => _api = api;

        private void AplicarToken()
        {
            var token = HttpContext.Session.GetString("JwtToken");
            if (!string.IsNullOrEmpty(token))
                _api.SetToken(token);
        }

        public async Task<IActionResult> OnGetAsync()
        {
            ViewData["Section"] = "Administración de Parámetros";
            ViewData["UserName"] = Request.Cookies["UserName"] ?? "Usuario de prueba";
            ViewData["UserRole"] = Request.Cookies["UserRole"] ?? "";
            ViewData["UserInitials"] = "U";
            try
            {
                AplicarToken();
                var result = await _api.GetAllAsync(1, 15);
                Parametros = result.Items;
            }
            catch (Exception ex) { MensajeError = $"Error al cargar: {ex.Message}"; }
            return Page();
        }

        public async Task<IActionResult> OnPostGuardarAsync([FromBody] ParametroInput input)
        {
            try
            {
                AplicarToken();
                var dto = new ParametroDto
                {
                    Identificador = input.Identificador ?? string.Empty,
                    Valor = input.Valor,
                    Estado = true
                };

                if (string.IsNullOrEmpty(input.Identificador))
                    return new JsonResult(new { exito = false, mensaje = "El identificador es requerido." }) { StatusCode = 400 };

                var exists = await _api.GetByIdAsync(dto.Identificador);
                if (exists == null)
                {
                    var (ok, error) = await _api.CreateAsync(dto);
                    if (ok) return new JsonResult(new { exito = true, mensaje = "Parámetro creado correctamente." });
                    return new JsonResult(new { exito = false, mensaje = error }) { StatusCode = 400 };
                }
                else
                {
                    var (ok, error) = await _api.UpdateAsync(dto);
                    if (ok) return new JsonResult(new { exito = true, mensaje = "Parámetro actualizado correctamente." });
                    return new JsonResult(new { exito = false, mensaje = error }) { StatusCode = 400 };
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new { exito = false, mensaje = ex.Message }) { StatusCode = 400 };
            }
        }

        public async Task<IActionResult> OnPostEliminarAsync([FromBody] string id)
        {
            try
            {
                AplicarToken();
                var ok = await _api.DeleteAsync(id);
                if (ok) return new JsonResult(new { exito = true, mensaje = "Parámetro eliminado correctamente." });
                return new JsonResult(new { exito = false, mensaje = "No se pudo eliminar." }) { StatusCode = 400 };
            }
            catch (Exception ex)
            {
                return new JsonResult(new { exito = false, mensaje = ex.Message }) { StatusCode = 400 };
            }
        }
    }
}