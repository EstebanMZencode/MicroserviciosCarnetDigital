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
        private readonly ITiposUsuarioApiClient _client;

        public List<TipoUsuarioDto> TiposUsuario { get; set; } = new();
        public string? MensajeError { get; set; }

        public TiposUsuarioModel(ITiposUsuarioApiClient client)
        {
            _client = client;
        }

        private void AplicarToken()
        {
            var token = HttpContext.Session.GetString("JwtToken");
            if (!string.IsNullOrEmpty(token))
                _client.SetToken(token);
        }

        public async Task<IActionResult> OnGetAsync()
        {
            CargarViewData();
            try
            {
                AplicarToken();
                TiposUsuario = await _client.ObtenerTodosAsync();
            }
            catch (HttpRequestException ex)
            {
                MensajeError = $"Error al cargar: {ex.StatusCode} - {ex.Message}";
            }
            return Page();
        }

        public async Task<IActionResult> OnPostGuardarAsync([FromBody] TipoUsuarioInput input)
        {
            if (string.IsNullOrWhiteSpace(input.Nombre))
                return new JsonResult(new { exito = false, mensaje = "El nombre no puede estar vacío." }) { StatusCode = 400 };

            try
            {
                AplicarToken();
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
                AplicarToken();
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