using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SitioAdministrativoCarnetDigital.Services.MicroServicioCarreras;

namespace SitioAdministrativoCarnetDigital.Pages.ModuloCarreras
{
    public class CarrerasModel : PageModel
    {
        private readonly ICarrerasApiClient _carrerasApi;

        public CarrerasModel(ICarrerasApiClient carrerasApi)
        {
            _carrerasApi = carrerasApi;
        }

        public List<CarreraDto> Carreras { get; set; } = new();
        public string? MensajeExito { get; set; }
        public string? MensajeError { get; set; }

        [BindProperty] public Guid CarreraID { get; set; }
        [BindProperty] public string NombreCarrera { get; set; } = string.Empty;
        [BindProperty] public string DirectorCarrera { get; set; } = string.Empty;
        [BindProperty] public string Email { get; set; } = string.Empty;
        [BindProperty] public string Telefono { get; set; } = string.Empty;
        [BindProperty] public Guid InstitucionID { get; set; }

        public async Task OnGetAsync()
        {
            try
            {
                Carreras = await _carrerasApi.GetAllAsync();
            }
            catch (Exception ex)
            {
                MensajeError = $"Error al cargar carreras: {ex.Message}";
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                var carrera = new CarreraDto
                {
                    CarreraID = CarreraID,
                    NombreCarrera = NombreCarrera,
                    DirectorCarrera = DirectorCarrera,
                    Email = Email,
                    Telefono = Telefono,
                    InstitucionID = InstitucionID
                };

                if (CarreraID == Guid.Empty)
                {
                    var (ok, status, msg) = await _carrerasApi.CreateAsync(carrera);
                    if (ok)
                        MensajeExito = "Carrera creada correctamente.";
                    else
                        MensajeError = $"Error al crear: {msg}";
                }
                else
                {
                    var ok = await _carrerasApi.UpdateAsync(carrera);
                    if (ok)
                        MensajeExito = "Carrera actualizada correctamente.";
                    else
                        MensajeError = "Error al actualizar.";
                }
            }
            catch (Exception ex)
            {
                MensajeError = $"Error: {ex.Message}";
            }

            Carreras = await _carrerasApi.GetAllAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostEliminarAsync()
        {
            try
            {
                var ok = await _carrerasApi.DeleteAsync(CarreraID);
                if (ok)
                    MensajeExito = "Carrera eliminada correctamente.";
                else
                    MensajeError = "Error al eliminar.";
            }
            catch (Exception ex)
            {
                MensajeError = $"Error: {ex.Message}";
            }

            Carreras = await _carrerasApi.GetAllAsync();
            return Page();
        }
    }
}