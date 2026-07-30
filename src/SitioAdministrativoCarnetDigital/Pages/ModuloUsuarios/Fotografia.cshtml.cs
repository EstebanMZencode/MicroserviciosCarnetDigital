using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SitioAdministrativoCarnetDigital.Services.MicroServicioFotografias;

namespace SitioAdministrativoCarnetDigital.Pages.ModuloUsuarios
{
    public class FotografiaModel : PageModel
    {
        private readonly IFotografiasApiClient _fotosApi;

        // ── Propiedades enlazadas al formulario ───────────────────────────────

        [BindProperty] public string  Correo      { get; set; } = string.Empty;
        // Base64 de la nueva fotografía; viene del campo oculto que rellena el JS.
        [BindProperty] public string? FotoBase64  { get; set; }

        // ── Estado de la vista ────────────────────────────────────────────────

        // Foto actual del usuario (null = no tiene foto / no se ha buscado aún).
        public string? FotoActualBase64 { get; private set; }
        // true cuando ya se realizó una búsqueda exitosa y debemos mostrar el panel de foto.
        public bool    MostrarPanel     { get; private set; }

        public string? MensajeExito     { get; private set; }
        public string? MensajeError     { get; private set; }

        public FotografiaModel(IFotografiasApiClient fotosApi)
        {
            _fotosApi = fotosApi;
        }

        // GET: página inicial vacía.
        public void OnGet() { }

        // POST handler "Buscar": consulta la foto del usuario por correo.
        public async Task OnPostBuscarAsync()
        {
            if (string.IsNullOrWhiteSpace(Correo))
            {
                MensajeError = "Debe indicar el correo del usuario.";
                return;
            }

            var token  = HttpContext.Session.GetString("JwtToken") ?? string.Empty;
            var result = await _fotosApi.ObtenerFotografiaAsync(token, Correo.Trim());

            if (!result.Success)
            {
                // 404 u otro error: mostrar mensaje, NO mostrar panel de foto.
                MensajeError = result.StatusCode == 404
                    ? $"No se encontró ningún usuario con el correo «{Correo.Trim()}»."
                    : result.ErrorMessage ?? "No se pudo obtener la fotografía.";
                return;
            }

            // Búsqueda exitosa: mostrar panel con foto (o avatar si viene vacía).
            MostrarPanel        = true;
            FotoActualBase64    = string.IsNullOrWhiteSpace(result.Data?.FotoBase64)
                                    ? null
                                    : result.Data!.FotoBase64;
        }

        // POST handler "Actualizar": recibe el Base64 del JS y llama al PATCH.
        public async Task<IActionResult> OnPostActualizarAsync()
        {
            if (string.IsNullOrWhiteSpace(Correo))
            {
                MensajeError = "El correo del usuario es obligatorio.";
                MostrarPanel = true;
                return Page();
            }

            if (string.IsNullOrWhiteSpace(FotoBase64))
            {
                MensajeError = "Debe seleccionar una fotografía.";
                MostrarPanel = true;
                return Page();
            }

            var token  = HttpContext.Session.GetString("JwtToken") ?? string.Empty;
            var result = await _fotosApi.ActualizarFotografiaAsync(
                token, Correo.Trim(), FotoBase64);

            // Tras la operación volvemos a buscar la foto para refrescar la vista.
            var getFoto = await _fotosApi.ObtenerFotografiaAsync(token, Correo.Trim());
            MostrarPanel     = true;
            FotoActualBase64 = getFoto.Success && !string.IsNullOrWhiteSpace(getFoto.Data?.FotoBase64)
                                ? getFoto.Data!.FotoBase64
                                : FotoBase64; // Si el GET falla mostramos la que acaba de subir

            if (result.Success)
                MensajeExito = "Fotografía actualizada correctamente.";
            else
                MensajeError = result.ErrorMessage ?? "No se pudo actualizar la fotografía.";

            return Page();
        }

        // POST handler "Eliminar": elimina la foto del usuario y muestra avatar.
        public async Task<IActionResult> OnPostEliminarAsync()
        {
            if (string.IsNullOrWhiteSpace(Correo))
            {
                MensajeError = "El correo del usuario es obligatorio.";
                return Page();
            }

            var token  = HttpContext.Session.GetString("JwtToken") ?? string.Empty;
            var result = await _fotosApi.EliminarFotografiaAsync(token, Correo.Trim());

            // Independientemente del resultado, mostramos el panel con avatar.
            MostrarPanel     = true;
            FotoActualBase64 = null;

            if (result.Success)
                MensajeExito = "Fotografía eliminada correctamente.";
            else
                MensajeError = result.ErrorMessage ?? "No se pudo eliminar la fotografía.";

            return Page();
        }
    }
}
