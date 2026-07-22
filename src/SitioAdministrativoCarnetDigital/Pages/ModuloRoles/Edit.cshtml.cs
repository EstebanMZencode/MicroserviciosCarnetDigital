using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SitioAdministrativoCarnetDigital.Services.MicroServicioRoles;
using System.Text.RegularExpressions;

namespace SitioAdministrativoCarnetDigital.Pages.ModuloRoles
{
    public class EditModel : PageModel
    {
        private readonly IRolesApiClient _rolesApi;

        // ID del rol que se edita: viene en el query string del GET
        // y como campo oculto en el POST.
        [BindProperty(SupportsGet = true)] public Guid        Id                    { get; set; }
        [BindProperty]                     public string       NombreRol             { get; set; } = string.Empty;
        [BindProperty]                     public List<Guid>   PantallasSeleccionadas { get; set; } = new();

        // Catálogo completo de pantallas para el checklist.
        public List<PantallaDto> Pantallas    { get; private set; } = new();
        public string?           ErrorMessage { get; private set; }

        public EditModel(IRolesApiClient rolesApi)
        {
            _rolesApi = rolesApi;
        }

        // Carga el rol y el catálogo de pantallas en paralelo para mejor rendimiento.
        public async Task<IActionResult> OnGetAsync()
        {
            var token = HttpContext.Session.GetString("JwtToken");
            if (string.IsNullOrEmpty(token))
                return RedirectToPage("/ModuloAuth/Login");

            if (Id == Guid.Empty)
                return RedirectToPage("Roles");

            // GET del rol y GET de pantallas en paralelo.
            var rolTask       = _rolesApi.GetRolAsync(token, Id);
            var pantallasTask = _rolesApi.GetPantallasAsync(token, pageSize: 200);
            await Task.WhenAll(rolTask, pantallasTask);

            var rolResult = rolTask.Result;
            if (!rolResult.Success || rolResult.Data is null)
            {
                TempData["ErrorMessage"] = rolResult.ErrorMessage ?? "No se encontró el rol.";
                return RedirectToPage("Roles");
            }

            // Pre-rellenar nombre y pantallas seleccionadas con los datos del rol.
            NombreRol             = rolResult.Data.NombreRol;
            PantallasSeleccionadas = rolResult.Data.Pantallas
                                               .Select(p => p.PantallaID)
                                               .ToList();

            var pantResult = pantallasTask.Result;
            Pantallas = pantResult.Success && pantResult.Data is not null
                ? pantResult.Data
                : new List<PantallaDto>();

            return Page();
        }

        // Valida y llama al PUT del microservicio. Redirige con TempData en éxito;
        // muestra la etiqueta de error inline en caso de fallo.
        public async Task<IActionResult> OnPostAsync()
        {
            var token = HttpContext.Session.GetString("JwtToken");
            if (string.IsNullOrEmpty(token))
                return RedirectToPage("/ModuloAuth/Login");

            if (Id == Guid.Empty)
                return RedirectToPage("Roles");

            if (string.IsNullOrWhiteSpace(NombreRol))
            {
                ErrorMessage = "El nombre del rol es obligatorio.";
                await CargarPantallasAsync(token);
                return Page();
            }

            if (NombreRol.Trim().Length > 40)
            {
                ErrorMessage = "El nombre del rol no puede superar los 40 caracteres.";
                await CargarPantallasAsync(token);
                return Page();
            }

            if (!Regex.IsMatch(NombreRol.Trim(), @"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s]+$"))
            {
                ErrorMessage = "El nombre del rol solo puede contener letras y espacios.";
                await CargarPantallasAsync(token);
                return Page();
            }

            var result = await _rolesApi.ActualizarRolAsync(
                token, Id, NombreRol.Trim(), PantallasSeleccionadas);

            if (result.Success)
            {
                TempData["SuccessMessage"] = $"Rol «{NombreRol.Trim()}» actualizado exitosamente.";
                return RedirectToPage("Roles");
            }

            ErrorMessage = result.ErrorMessage ?? "No se pudo actualizar el rol.";
            await CargarPantallasAsync(token);
            return Page();
        }

        private async Task CargarPantallasAsync(string token)
        {
            var result = await _rolesApi.GetPantallasAsync(token, pageSize: 200);
            Pantallas = result.Success && result.Data is not null
                ? result.Data
                : new List<PantallaDto>();
        }
    }
}
