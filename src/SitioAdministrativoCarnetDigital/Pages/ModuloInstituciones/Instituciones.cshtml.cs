using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SitioAdministrativoCarnetDigital.Services.MicroServicioInstituciones;

namespace SitioAdministrativoCarnetDigital.Pages.ModuloInstituciones
{
    public class InstitucionesModel : PageModel
    {
        private readonly IInstitucionesApiClient _institucionesApi;

        public InstitucionesModel(IInstitucionesApiClient institucionesApi)
        {
            _institucionesApi = institucionesApi;
        }

        public List<InstitucionDto> Instituciones { get; set; } = new();
        public string? MensajeExito { get; set; }
        public string? MensajeError { get; set; }

        [BindProperty] public Guid InstitucionID { get; set; }
        [BindProperty] public string Nombre { get; set; } = string.Empty;
        [BindProperty] public string Email { get; set; } = string.Empty;
        [BindProperty] public string Telefono { get; set; } = string.Empty;
        [BindProperty] public string Dominios { get; set; } = string.Empty;

        private void AplicarToken()
        {
            var token = HttpContext.Session.GetString("JwtToken");
            if (!string.IsNullOrEmpty(token))
                _institucionesApi.SetToken(token);
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
                Instituciones = await _institucionesApi.GetAllAsync();
            }
            catch (Exception ex)
            {
                MensajeError = $"Error al cargar instituciones: {ex.Message}";
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                AplicarToken();
                var institucion = new InstitucionDto
                {
                    InstitucionID = InstitucionID,
                    NombreInstitucion = Nombre,
                    Email = Email,
                    Telefono = Telefono,
                    Dominios = Dominios
                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(d => d.Trim())
                        .ToList()
                };

                if (InstitucionID == Guid.Empty)
                {
                    var (ok, status, msg) = await _institucionesApi.CreateAsync(institucion);
                    if (ok)
                        MensajeExito = "Institución creada correctamente.";
                    else
                        MensajeError = $"Error al crear: {msg}";
                }
                else
                {
                    var ok = await _institucionesApi.UpdateAsync(institucion);
                    if (ok)
                        MensajeExito = "Institución actualizada correctamente.";
                    else
                        MensajeError = "Error al actualizar.";
                }
            }
            catch (Exception ex)
            {
                MensajeError = $"Error: {ex.Message}";
            }

            AplicarToken();
            Instituciones = await _institucionesApi.GetAllAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostEliminarAsync()
        {
            try
            {
                AplicarToken();
                var ok = await _institucionesApi.DeleteAsync(InstitucionID);
                if (ok)
                    MensajeExito = "Institución eliminada correctamente.";
                else
                    MensajeError = "Error al eliminar.";
            }
            catch (Exception ex)
            {
                MensajeError = $"Error: {ex.Message}";
            }

            AplicarToken();
            Instituciones = await _institucionesApi.GetAllAsync();
            return Page();
        }
    }
}