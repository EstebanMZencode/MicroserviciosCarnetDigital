using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SitioAdministrativoCarnetDigital.Services.MicroServicioCarreras;
using SitioAdministrativoCarnetDigital.Services.MicroServicioInstituciones;

namespace SitioAdministrativoCarnetDigital.Pages.ModuloCarreras
{
    public class CarreraInput
    {
        public Guid CarreraID { get; set; }
        public string NombreCarrera { get; set; } = string.Empty;
        public string DirectorCarrera { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public Guid InstitucionID { get; set; }
    }

    public class EliminarCarreraInput
    {
        public Guid Id { get; set; }
    }

    public class CarrerasModel : PageModel
    {
        private readonly ICarrerasApiClient _carrerasApi;
        private readonly IInstitucionesApiClient _institucionesApi;

        public CarrerasModel(ICarrerasApiClient carrerasApi, IInstitucionesApiClient institucionesApi)
        {
            _carrerasApi = carrerasApi;
            _institucionesApi = institucionesApi;
        }

        public List<CarreraDto> Carreras { get; set; } = new();
        public List<InstitucionDto> Instituciones { get; set; } = new();
        public string? MensajeExito { get; set; }
        public string? MensajeError { get; set; }

        private void AplicarToken()
        {
            var token = HttpContext.Session.GetString("JwtToken");
            if (!string.IsNullOrEmpty(token))
            {
                _carrerasApi.SetToken(token);
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
                Carreras = await _carrerasApi.GetAllAsync();
                Instituciones = await _institucionesApi.GetAllAsync();
            }
            catch (Exception ex)
            {
                MensajeError = $"Error al cargar: {ex.Message}";
            }
        }

        public async Task<IActionResult> OnPostGuardarAsync([FromBody] CarreraInput input)
        {
            try
            {
                AplicarToken();
                var carrera = new CarreraDto
                {
                    CarreraID = input.CarreraID,
                    NombreCarrera = input.NombreCarrera,
                    DirectorCarrera = input.DirectorCarrera,
                    Email = input.Email,
                    Telefono = input.Telefono,
                    InstitucionID = input.InstitucionID
                };

                if (input.CarreraID == Guid.Empty)
                {
                    var (ok, status, msg) = await _carrerasApi.CreateAsync(carrera);
                    if (ok) return new JsonResult(new { exito = true, mensaje = "Carrera creada correctamente." });
                    return new JsonResult(new { exito = false, mensaje = msg }) { StatusCode = 400 };
                }
                else
                {
                    var ok = await _carrerasApi.UpdateAsync(carrera);
                    if (ok) return new JsonResult(new { exito = true, mensaje = "Carrera actualizada correctamente." });
                    return new JsonResult(new { exito = false, mensaje = "Error al actualizar." }) { StatusCode = 400 };
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new { exito = false, mensaje = ex.Message }) { StatusCode = 400 };
            }
        }

        public async Task<IActionResult> OnPostEliminarAsync([FromBody] EliminarCarreraInput input)
        {
            try
            {
                AplicarToken();
                var ok = await _carrerasApi.DeleteAsync(input.Id);
                if (ok) return new JsonResult(new { exito = true, mensaje = "Carrera eliminada correctamente." });
                return new JsonResult(new { exito = false, mensaje = "No se pudo eliminar." }) { StatusCode = 400 };
            }
            catch (Exception ex)
            {
                return new JsonResult(new { exito = false, mensaje = ex.Message }) { StatusCode = 400 };
            }
        }
    }
}