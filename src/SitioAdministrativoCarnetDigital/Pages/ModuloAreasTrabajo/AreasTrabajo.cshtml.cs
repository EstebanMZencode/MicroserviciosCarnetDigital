using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SitioAdministrativoCarnetDigital.Services.MicroServicioAreasTrabajo;

namespace SitioAdministrativoCarnetDigital.Pages.ModuloAreasTrabajo
{
    public class AreasTrabajoModel : PageModel
    {
        private readonly IAreasTrabajoApiClient _areasApi;

        public AreasTrabajoModel(IAreasTrabajoApiClient areasApi)
        {
            _areasApi = areasApi;
        }

        public List<AreaTrabajoDto> Areas { get; set; } = new();
        public string? MensajeExito { get; set; }
        public string? MensajeError { get; set; }

        [BindProperty] public Guid AreaTrabID { get; set; }
        [BindProperty] public string NombreAreaTrab { get; set; } = string.Empty;
        [BindProperty] public Guid InstitucionID { get; set; }

        private void AplicarToken()
        {
            var token = HttpContext.Session.GetString("JwtToken");
            if (!string.IsNullOrEmpty(token))
                _areasApi.SetToken(token);
            var email = HttpContext.Session.GetString("UserEmail") ?? "Usuario";
            ViewData["UserName"] = email;
            ViewData["UserRole"] = "Administrador";
            ViewData["UserInitials"] = email.Length > 0 ? email[0].ToString().ToUpper() : "U";
        }

        public async Task OnGetAsync()
        {
            try
            {
                AplicarToken();
                Areas = await _areasApi.GetAllAsync();
            }
            catch (Exception ex)
            {
                MensajeError = $"Error al cargar áreas: {ex.Message}";
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                AplicarToken();
                var area = new AreaTrabajoDto
                {
                    AreaTrabID = AreaTrabID,
                    NombreAreaTrab = NombreAreaTrab,
                    InstitucionID = InstitucionID
                };

                if (AreaTrabID == Guid.Empty)
                {
                    var (ok, status, msg) = await _areasApi.CreateAsync(area);
                    if (ok)
                        MensajeExito = "Área creada correctamente.";
                    else
                        MensajeError = $"Error al crear: {msg}";
                }
                else
                {
                    var ok = await _areasApi.UpdateAsync(area);
                    if (ok)
                        MensajeExito = "Área actualizada correctamente.";
                    else
                        MensajeError = "Error al actualizar.";
                }
            }
            catch (Exception ex)
            {
                MensajeError = $"Error: {ex.Message}";
            }

            AplicarToken();
            Areas = await _areasApi.GetAllAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostEliminarAsync()
        {
            try
            {
                AplicarToken();
                var ok = await _areasApi.DeleteAsync(AreaTrabID);
                if (ok)
                    MensajeExito = "Área eliminada correctamente.";
                else
                    MensajeError = "Error al eliminar.";
            }
            catch (Exception ex)
            {
                MensajeError = $"Error: {ex.Message}";
            }

            AplicarToken();
            Areas = await _areasApi.GetAllAsync();
            return Page();
        }
    }
}