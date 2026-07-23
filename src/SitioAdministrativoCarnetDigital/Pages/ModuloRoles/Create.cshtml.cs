using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SitioAdministrativoCarnetDigital.Services.MicroServicioRoles;
using System.Text.RegularExpressions;

namespace SitioAdministrativoCarnetDigital.Pages.ModuloRoles
{
    public class CreateModel : PageModel
    {
        private readonly IRolesApiClient _rolesApi;

        // GUID generado en el GET y pasado como campo oculto en el POST,
        // porque el endpoint POST de MicroServicioRoles lo recibe en el header.
        [BindProperty] public Guid NuevoRolId { get; set; }
        [BindProperty] public string NombreRol { get; set; } = string.Empty;
        [BindProperty] public List<Guid> PantallasSeleccionadas { get; set; } = new();

        // Lista completa de pantallas para armar el checklist.
        public List<PantallaDto> Pantallas { get; private set; } = new();
        public string? ErrorMessage { get; private set; }

        public CreateModel(IRolesApiClient rolesApi)
        {
            _rolesApi = rolesApi;
        }

        // Genera el GUID del nuevo rol y carga el catálogo de pantallas.
        public async Task<IActionResult> OnGetAsync()
        {
            var token = HttpContext.Session.GetString("JwtToken");
            if (string.IsNullOrEmpty(token))
                return RedirectToPage("/ModuloAuth/Login");

            NuevoRolId = Guid.NewGuid();
            await CargarPantallasAsync(token);
            return Page();
        }

        // Valida los datos, llama al microservicio y redirige o muestra el error.
        public async Task<IActionResult> OnPostAsync()
        {
            var token = HttpContext.Session.GetString("JwtToken");
            if (string.IsNullOrEmpty(token))
                return RedirectToPage("/ModuloAuth/Login");

            // Validaciones de negocio antes de llamar al microservicio.
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

            // Solo letras (incluyendo español) y espacios.
            if (!Regex.IsMatch(NombreRol.Trim(), @"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s]+$"))
            {
                ErrorMessage = "El nombre del rol solo puede contener letras y espacios.";
                await CargarPantallasAsync(token);
                return Page();
            }

            var result = await _rolesApi.CrearRolAsync(
                token, NuevoRolId, NombreRol.Trim(), PantallasSeleccionadas);

            if (result.Success)
            {
                TempData["SuccessMessage"] = $"Rol «{NombreRol.Trim()}» creado exitosamente.";
                return RedirectToPage("Roles");
            }

            // El mensaje de error viene del body del microservicio.
            ErrorMessage = result.ErrorMessage ?? "No se pudo crear el rol.";
            await CargarPantallasAsync(token);
            return Page();
        }

        // Consulta el catálogo de pantallas a través de RolesApiClient
        // (que internamente llama a MicroServicioPantallas).
        private async Task CargarPantallasAsync(string token)
        {
            var result = await _rolesApi.GetPantallasAsync(token, pageSize: 200);
            Pantallas = result.Success && result.Data is not null
                ? result.Data
                : new List<PantallaDto>();
        }
    }
}

