using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SitioAdministrativoCarnetDigital.Services.MicroServicioEstadosUsuario;

namespace SitioAdministrativoCarnetDigital.Pages.ModuloEstadoUsuario
{
    public class CambiarEstadoInput
    {
        public string EmailUsuario { get; set; } = string.Empty;
        public Guid EstadoID { get; set; }
    }

    public class EstadosModel : PageModel
    {
        private readonly IEstadosUsuarioApiClient _client;

        public EstadosModel(IEstadosUsuarioApiClient client)
        {
            _client = client;
        }

        public void OnGet()
        {
            
            // BYPASS TEMPORAL — comentado mientras no existe el login del
            // sitio. REACTIVAR cuando el login definitivo esté listo.
            // ============================================================
            // var tokenSesion = Request.Cookies["JWToken"];
            // if (string.IsNullOrEmpty(tokenSesion))
            // {
            //     TempData["MensajeLogin"] = "Por favor inicie sesión para utilizar el sistema";
            //     Response.Redirect("/Login");
            //     return;
            // }

            ViewData["Section"] = "Cambiar Estado de Usuario";
            ViewData["UserName"] = Request.Cookies["UserName"] ?? "Usuario de prueba";
            ViewData["UserRole"] = Request.Cookies["UserRole"] ?? "";
            ViewData["UserInitials"] = "U";
        }

        public async Task<IActionResult> OnPostCambiarAsync([FromBody] CambiarEstadoInput input)
        {
            if (string.IsNullOrWhiteSpace(input.EmailUsuario))
                return new JsonResult(new { exito = false, mensaje = "El email del usuario es obligatorio." }) { StatusCode = 400 };

            if (input.EstadoID == Guid.Empty)
                return new JsonResult(new { exito = false, mensaje = "Debe seleccionar un estado." }) { StatusCode = 400 };

            try
            {
                await _client.CambiarEstadoAsync(input.EmailUsuario, input.EstadoID);
                return new JsonResult(new { exito = true, mensaje = "Estado actualizado correctamente." });
            }
            catch (HttpRequestException ex)
            {
                return new JsonResult(new { exito = false, mensaje = ex.Message }) { StatusCode = 400 };
            }
        }
    }
}