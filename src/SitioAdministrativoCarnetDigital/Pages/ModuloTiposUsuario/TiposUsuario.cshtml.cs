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
        
        // TOKEN DE PRUEBA TEMPORAL — pegá acá el "access_token" (o el
        // campo que corresponda) que te devuelve Postman al hacer login
        // contra MicroServicioAuth/api/login. Se usa solo si no hay
        // cookie "JWToken" real. BORRAR/vaciar esta constante en cuanto
        // el login definitivo esté funcionando.
        
        private const string TokenPrueba = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJqaW1lbmV6QXJyaWV0QGdtYWlsLmNvbSIsImVtYWlsIjoiamltZW5lekFycmlldEBnbWFpbC5jb20iLCJqdGkiOiIyYjljN2VhZC1hMjI4LTQ1NDYtOGYzMS03MmQ3MTk2NmNkN2IiLCJyb2wiOiJlMjBhYzMxYy05MzI5LTRmYTAtODllYi02NTE1ZWY5MTY1MTkiLCJ0aXBvX3VzdWFyaW8iOiIwYjRkN2RhMC0xNDM4LTQzNmMtYjJkNC0yZDNhOGYzMDA1ZTciLCJleHAiOjE3ODQ3NjMxNzcsImlzcyI6Ik1pY3JvU2VydmljaW9BdXRoIiwiYXVkIjoiQ2FybmV0RXN0dWRpYW50aWxEaWdpdGFsIn0.iQ6IKoXIpXfJhu_6qA5efPNVKq2b_ZpiFoLJQsxEBA0";

        private readonly ITiposUsuarioApiClient _client;

        public List<TipoUsuarioDto> TiposUsuario { get; set; } = new();
        public string? MensajeError { get; set; }

        public TiposUsuarioModel(ITiposUsuarioApiClient client)
        {
            _client = client;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var token = Request.Cookies["JWToken"] ?? TokenPrueba;

            
            // BYPASS TEMPORAL — comentado para poder ver el diseño mientras
            // no existe el login. REACTIVAR en cuanto el login definitivo
            // esté listo (descomentar). Sin token real, la llamada al
            // microservicio va a fallar con 401 y vas a ver el mensaje de
            // error en vez del listado — es esperado, es solo para ver el
            // diseño/maquetación.
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
            var token = Request.Cookies["JWToken"] ?? TokenPrueba;
            // BYPASS TEMPORAL — ver nota en OnGetAsync. Reactivar validación cuando exista login real.
            // if (string.IsNullOrEmpty(token))
            //     return new JsonResult(new { exito = false, mensaje = "Sesión expirada." }) { StatusCode = 401 };

            if (string.IsNullOrWhiteSpace(input.Nombre))
                return new JsonResult(new { exito = false, mensaje = "El nombre no puede estar vacío." }) { StatusCode = 400 };

            _client.SetToken(token ?? string.Empty);

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
            var token = Request.Cookies["JWToken"] ?? TokenPrueba;
            // BYPASS TEMPORAL — ver nota en OnGetAsync. Reactivar validación cuando exista login real.
            // if (string.IsNullOrEmpty(token))
            //     return new JsonResult(new { exito = false, mensaje = "Sesión expirada." }) { StatusCode = 401 };

            _client.SetToken(token ?? string.Empty);

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