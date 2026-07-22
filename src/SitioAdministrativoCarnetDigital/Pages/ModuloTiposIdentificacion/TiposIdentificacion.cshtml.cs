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
        
        // TOKEN DE PRUEBA TEMPORAL — pegá acá el "access_token" (o el
        // campo que corresponda) que te devuelve Postman al hacer login
        // contra MicroServicioAuth/api/login. Se usa solo si no hay
        // cookie "JWToken" real. BORRAR/vaciar esta constante en cuanto
        // el login definitivo esté funcionando.
        
        private const string TokenPrueba = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJqaW1lbmV6QXJyaWV0QGdtYWlsLmNvbSIsImVtYWlsIjoiamltZW5lekFycmlldEBnbWFpbC5jb20iLCJqdGkiOiIzOGFkNzI2Mi1iOGUyLTQ2ZTgtODhhNS04NzE3OTQ2NWJhODMiLCJyb2wiOiJlMjBhYzMxYy05MzI5LTRmYTAtODllYi02NTE1ZWY5MTY1MTkiLCJ0aXBvX3VzdWFyaW8iOiIwYjRkN2RhMC0xNDM4LTQzNmMtYjJkNC0yZDNhOGYzMDA1ZTciLCJleHAiOjE3ODQ3NjE5MjAsImlzcyI6Ik1pY3JvU2VydmljaW9BdXRoIiwiYXVkIjoiQ2FybmV0RXN0dWRpYW50aWxEaWdpdGFsIn0._IdL11fQZgmsHOX2gE-BxOsGUP_xWqGneo8ZMkhQAck";

        private readonly ITiposIdentificacionApiClient _client;

        public List<TipoIdentificacionDto> TiposIdentificacion { get; set; } = new();
        public string? MensajeError { get; set; }

        public TiposIdentificacionModel(ITiposIdentificacionApiClient client)
        {
            _client = client;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var token = Request.Cookies["JWToken"] ?? TokenPrueba;

            
            // BYPASS TEMPORAL — comentado para poder ver el diseño mientras
            // no existe el login. REACTIVAR en cuanto el login definitivo
            // esté listo (descomentar). Sin token real, la llamada al
            // microservicio va a fallar con 401 — es esperado en modo bypass.
            // ============================================================
            // if (string.IsNullOrEmpty(token))
            // {
            //     TempData["MensajeLogin"] = "Por favor inicie sesión para utilizar el sistema";
            //     return RedirectToPage("/Login");
            // }

            CargarViewData();
            _client.SetToken(token ?? string.Empty);

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
            var token = Request.Cookies["JWToken"] ?? TokenPrueba;
            // BYPASS TEMPORAL — ver nota en OnGetAsync. Reactivar validación cuando exista login real.
            // if (string.IsNullOrEmpty(token))
            //     return new JsonResult(new { exito = false, mensaje = "Sesión expirada." }) { StatusCode = 401 };

            if (string.IsNullOrWhiteSpace(input.Nombre))
                return new JsonResult(new { exito = false, mensaje = "El nombre no puede estar vacío." }) { StatusCode = 400 };

            _client.SetToken(token ?? string.Empty);

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
            var token = Request.Cookies["JWToken"] ?? TokenPrueba;
            // BYPASS TEMPORAL — ver nota en OnGetAsync. Reactivar validación cuando exista login real.
            // if (string.IsNullOrEmpty(token))
            //     return new JsonResult(new { exito = false, mensaje = "Sesión expirada." }) { StatusCode = 401 };

            _client.SetToken(token ?? string.Empty);

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