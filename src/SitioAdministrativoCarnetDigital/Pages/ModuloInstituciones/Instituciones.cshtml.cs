using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SitioAdministrativoCarnetDigital.Services.MicroServicioInstituciones;

namespace SitioAdministrativoCarnetDigital.Pages.ModuloInstituciones
{
    public class InstitucionInput
    {
        public Guid InstitucionID { get; set; }
        public string NombreInstitucion { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Dominios { get; set; } = string.Empty;
    }

    public class EliminarInstitucionInput
    {
        public Guid Id { get; set; }
    }

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

        public async Task<IActionResult> OnPostGuardarAsync([FromBody] InstitucionInput input)
        {
            try
            {
                AplicarToken();
                var institucion = new InstitucionDto
                {
                    InstitucionID = input.InstitucionID,
                    NombreInstitucion = input.NombreInstitucion,
                    Email = input.Email,
                    Telefono = input.Telefono,
                    Dominios = input.Dominios
                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(d => d.Trim())
                        .ToList()
                };

                if (input.InstitucionID == Guid.Empty)
                {
                    var (ok, status, msg) = await _institucionesApi.CreateAsync(institucion);
                    if (ok) return new JsonResult(new { exito = true, mensaje = "Institución creada correctamente." });
                    return new JsonResult(new { exito = false, mensaje = msg }) { StatusCode = 400 };
                }
                else
                {
                    var ok = await _institucionesApi.UpdateAsync(institucion);
                    if (ok) return new JsonResult(new { exito = true, mensaje = "Institución actualizada correctamente." });
                    return new JsonResult(new { exito = false, mensaje = "Error al actualizar." }) { StatusCode = 400 };
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new { exito = false, mensaje = ex.Message }) { StatusCode = 400 };
            }
        }

        public async Task<IActionResult> OnPostEliminarAsync([FromBody] EliminarInstitucionInput input)
        {
            try
            {
                AplicarToken();
                var ok = await _institucionesApi.DeleteAsync(input.Id);
                if (ok) return new JsonResult(new { exito = true, mensaje = "Institución eliminada correctamente." });
                return new JsonResult(new { exito = false, mensaje = "No se pudo eliminar." }) { StatusCode = 400 };
            }
            catch (Exception ex)
            {
                return new JsonResult(new { exito = false, mensaje = ex.Message }) { StatusCode = 400 };
            }
        }
    }
}