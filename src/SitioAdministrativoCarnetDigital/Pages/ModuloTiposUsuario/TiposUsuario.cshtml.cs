using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SitioAdministrativoCarnetDigital.Services.MicroServicioTiposUsuario;

namespace SitioAdministrativoCarnetDigital.Pages.ModuloTiposUsuario
{
    public class TipoUsuarioInput
    {
        public Guid? TipoUsuarioID { get; set; }
        public string Nombre { get; set; } = string.Empty;
    }

    public class TiposUsuarioModel : PageModel
    {
        // El login hacia el microservicio ya no se maneja acá — el propio
        // ITiposUsuarioApiClient se loguea solo 
        // Esto es temporal, mientras no exista el login real del equipo.
        private readonly ITiposUsuarioApiClient _client;

        public List<TipoUsuarioDto> TiposUsuario { get; set; } = new();
        public string? MensajeError { get; set; }

        public TiposUsuarioModel(ITiposUsuarioApiClient client)
        {
            _client = client;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            
            // BYPASS TEMPORAL 
            // no existe el login del SITIO (esto es la sesión del usuario
            // viendo esta pantalla, distinto del login automático hacia el
            // microservicio). REACTIVAR en cuanto el login definitivo esté
            // listo (descomentar).
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
                TiposUsuario = await _client.ObtenerTodosAsync();
            }
            catch (HttpRequestException ex)
            {
                MensajeError = $"Error al cargar: {ex.StatusCode} - {ex.Message}";
            }

            return Page();
        }

        // Handler unico para crear/actualizar: si no viene TipoUsuarioID, es creacion (POST);
        // si viene, es actualizacion (PUT).
        public async Task<IActionResult> OnPostGuardarAsync([FromBody] TipoUsuarioInput input)
        {
            if (string.IsNullOrWhiteSpace(input.Nombre))
                return new JsonResult(new { exito = false, mensaje = "El nombre no puede estar vacío." }) { StatusCode = 400 };

            try
            {
                if (input.TipoUsuarioID is null || input.TipoUsuarioID == Guid.Empty)
                    await _client.CrearAsync(input.Nombre);
                else
                    await _client.ActualizarAsync(input.TipoUsuarioID.Value, input.Nombre);

                return new JsonResult(new { exito = true, mensaje = "Tipo de usuario guardado correctamente." });
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
                return new JsonResult(new { exito = true, mensaje = "Tipo de usuario eliminado correctamente." });
            }
            catch (HttpRequestException ex)
            {
                return new JsonResult(new { exito = false, mensaje = ex.Message }) { StatusCode = 400 };
            }
        }

        private void CargarViewData()
        {
            ViewData["Section"] = "Tipos de Usuario";
            ViewData["UserName"] = Request.Cookies["UserName"] ?? "Usuario de prueba";
            ViewData["UserRole"] = Request.Cookies["UserRole"] ?? "";
            ViewData["UserInitials"] = "U";
        }
    }
}