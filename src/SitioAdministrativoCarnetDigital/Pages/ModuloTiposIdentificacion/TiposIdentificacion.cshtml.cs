using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SitioAdministrativoCarnetDigital.Services.MicroServicioTiposIdentificacion;

namespace SitioAdministrativoCarnetDigital.Pages.ModuloTiposIdentificacion
{
    public class TipoIdentificacionInput
    {
        public Guid? TipoIdentificacionID { get; set; }
        public string Nombre { get; set; } = string.Empty;
    }

    public class TiposIdentificacionModel : PageModel
    {
        // El login hacia el microservicio ya no se maneja acá — el propio
        // ITiposIdentificacionApiClient se loguea solo 
        // Esto es temporal, mientras no exista el login real del equipo.
        private readonly ITiposIdentificacionApiClient _client;

        public List<TipoIdentificacionDto> TiposIdentificacion { get; set; } = new();
        public string? MensajeError { get; set; }

        public TiposIdentificacionModel(ITiposIdentificacionApiClient client)
        {
            _client = client;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            
            // BYPASS TEMPORAL 
            // no existe el login del SITIO (distinto del login automático
            // hacia el microservicio). REACTIVAR en cuanto el login
            // definitivo esté listo (descomentar).
            // ============================================================
            // var tokenSesion = Request.Cookies["JWToken"];
            // if (string.IsNullOrEmpty(tokenSesion))
            // {
            //     TempData["MensajeLogin"] = "Por favor inicie sesión para utilizar el sistema";
            //     return RedirectToPage("/Login");
            // }

            CargarViewData();

            try
            {
                TiposIdentificacion = await _client.ObtenerTodosAsync();
            }
            catch (HttpRequestException ex)
            {
                MensajeError = $"Error al cargar: {ex.StatusCode} - {ex.Message}";
            }

            return Page();
        }

        public async Task<IActionResult> OnPostGuardarAsync([FromBody] TipoIdentificacionInput input)
        {
            if (string.IsNullOrWhiteSpace(input.Nombre))
                return new JsonResult(new { exito = false, mensaje = "El nombre no puede estar vacío." }) { StatusCode = 400 };

            try
            {
                if (input.TipoIdentificacionID is null || input.TipoIdentificacionID == Guid.Empty)
                    await _client.CrearAsync(input.Nombre);
                else
                    await _client.ActualizarAsync(input.TipoIdentificacionID.Value, input.Nombre);

                return new JsonResult(new { exito = true, mensaje = "Tipo de identificación guardado correctamente." });
            }
            catch (HttpRequestException ex)
            {
                return new JsonResult(new { exito = false, mensaje = ex.Message }) { StatusCode = 400 };
            }
        }

        public async Task<IActionResult> OnPostEliminarAsync([FromBody] Guid id)
        {
            try
            {
                await _client.EliminarAsync(id);
                return new JsonResult(new { exito = true, mensaje = "Tipo de identificación eliminado correctamente." });
            }
            catch (HttpRequestException ex)
            {
                return new JsonResult(new { exito = false, mensaje = ex.Message }) { StatusCode = 400 };
            }
        }

        private void CargarViewData()
        {
            ViewData["Section"] = "Tipos de Identificación";
            ViewData["UserName"] = Request.Cookies["UserName"] ?? "Usuario de prueba";
            ViewData["UserRole"] = Request.Cookies["UserRole"] ?? "";
            ViewData["UserInitials"] = "U";
        }
    }
}