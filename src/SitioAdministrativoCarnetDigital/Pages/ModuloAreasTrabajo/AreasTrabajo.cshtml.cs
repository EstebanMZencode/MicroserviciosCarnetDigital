using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SitioAdministrativoCarnetDigital.Services.MicroServicioAreasTrabajo;
using SitioAdministrativoCarnetDigital.Services.MicroServicioInstituciones;

namespace SitioAdministrativoCarnetDigital.Pages.ModuloAreasTrabajo
{
    public class AreaInput
    {
        public Guid AreaTrabID { get; set; }
        public string NombreAreaTrab { get; set; } = string.Empty;
        public Guid InstitucionID { get; set; }
    }

    public class EliminarAreaInput
    {
        public Guid Id { get; set; }
    }

    public class AreasTrabajoModel : PageModel
    {
        private readonly IAreasTrabajoApiClient _areasApi;
        private readonly IInstitucionesApiClient _institucionesApi;

        public AreasTrabajoModel(IAreasTrabajoApiClient areasApi, IInstitucionesApiClient institucionesApi)
        {
            _areasApi = areasApi;
            _institucionesApi = institucionesApi;
        }

        public List<AreaTrabajoDto> Areas { get; set; } = new();
        public List<InstitucionDto> Instituciones { get; set; } = new();
        public string? MensajeExito { get; set; }
        public string? MensajeError { get; set; }

        private void AplicarToken()
        {
            var token = HttpContext.Session.GetString("JwtToken");
            if (!string.IsNullOrEmpty(token))
            {
                _areasApi.SetToken(token);
                _institucionesApi.SetToken(token);
            }
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
                Instituciones = await _institucionesApi.GetAllAsync();
            }
            catch (Exception ex)
            {
                MensajeError = $"Error al cargar: {ex.Message}";
            }
        }

        public async Task<IActionResult> OnPostGuardarAsync([FromBody] AreaInput input)
        {
            try
            {
                AplicarToken();
                var area = new AreaTrabajoDto
                {
                    AreaTrabID = input.AreaTrabID,
                    NombreAreaTrab = input.NombreAreaTrab,
                    InstitucionID = input.InstitucionID
                };

                if (input.AreaTrabID == Guid.Empty)
                {
                    var (ok, status, msg) = await _areasApi.CreateAsync(area);
                    if (ok) return new JsonResult(new { exito = true, mensaje = "Área creada correctamente." });
                    return new JsonResult(new { exito = false, mensaje = msg }) { StatusCode = 400 };
                }
                else
                {
                    var ok = await _areasApi.UpdateAsync(area);
                    if (ok) return new JsonResult(new { exito = true, mensaje = "Área actualizada correctamente." });
                    return new JsonResult(new { exito = false, mensaje = "Error al actualizar." }) { StatusCode = 400 };
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new { exito = false, mensaje = ex.Message }) { StatusCode = 400 };
            }
        }

        public async Task<IActionResult> OnPostEliminarAsync([FromBody] EliminarAreaInput input)
        {
            try
            {
                AplicarToken();
                var ok = await _areasApi.DeleteAsync(input.Id);
                if (ok) return new JsonResult(new { exito = true, mensaje = "Área eliminada correctamente." });
                return new JsonResult(new { exito = false, mensaje = "No se pudo eliminar." }) { StatusCode = 400 };
            }
            catch (Exception ex)
            {
                return new JsonResult(new { exito = false, mensaje = ex.Message }) { StatusCode = 400 };
            }
        }
    }
}