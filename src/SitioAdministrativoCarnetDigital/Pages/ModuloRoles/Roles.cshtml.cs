using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SitioAdministrativoCarnetDigital.Services.MicroServicioRoles;
using System.Text.RegularExpressions;

namespace SitioAdministrativoCarnetDigital.Pages.ModuloRoles
{
    public class RolesModel : PageModel
    {
        private readonly IRolesApiClient _rolesApi;
        private readonly IConfiguration _config;

        public List<RolDto> Roles { get; private set; } = new();
        public List<PantallaDto> Pantallas { get; private set; } = new();
        public int PaginaActual { get; private set; } = 1;
        public int TotalPaginas { get; private set; } = 1;
        public int TotalRegistros { get; private set; } = 0;
        public string? SuccessMessage { get; private set; }
        public string? ErrorMessage { get; private set; }

        [BindProperty] public string NombreRol { get; set; } = string.Empty;
        [BindProperty] public Guid RolID { get; set; }
        [BindProperty] public List<Guid> PantallasSeleccionadas { get; set; } = new();

        public RolesModel(IRolesApiClient rolesApi, IConfiguration config)
        {
            _rolesApi = rolesApi;
            _config = config;
        }

        public async Task<IActionResult> OnGetAsync(int pagina = 1)
        {
            var token = HttpContext.Session.GetString("JwtToken");
            if (string.IsNullOrEmpty(token))
                return RedirectToPage("/ModuloAuth/Login");

            SuccessMessage = TempData["SuccessMessage"]?.ToString();
            ErrorMessage = TempData["ErrorMessage"]?.ToString();

            var pageSize = _config.GetValue<int>("Pagination", 15);
            PaginaActual = pagina < 1 ? 1 : pagina;

            var rolesTask = _rolesApi.GetRolesAsync(token, PaginaActual, pageSize);
            var pantallasTask = _rolesApi.GetPantallasAsync(token, 200);
            await Task.WhenAll(rolesTask, pantallasTask);

            var rolesResult = rolesTask.Result;
            if (rolesResult.Success && rolesResult.Data is not null)
            {
                Roles = rolesResult.Data.Roles;
                TotalRegistros = rolesResult.Data.TotalRegistros;

                // El microservicio devuelve "totalRegistros" pero NO "totalPaginas".
                // Se calcula aquÌ con ceiling division para no depender de un campo
                // que nunca viene en la respuesta y siempre deserializa como 0.
                TotalPaginas = TotalRegistros <= 0
                    ? 1
                    : (int)Math.Ceiling((double)TotalRegistros / pageSize);
            }
            else
            {
                ErrorMessage = rolesResult.ErrorMessage ?? "No se pudieron cargar los roles.";
            }

            var pantallasResult = pantallasTask.Result;
            if (pantallasResult.Success && pantallasResult.Data is not null)
                Pantallas = pantallasResult.Data;

            return Page();
        }

        public async Task<IActionResult> OnPostCrearAsync(int pagina = 1)
        {
            var token = HttpContext.Session.GetString("JwtToken");
            if (string.IsNullOrEmpty(token))
                return RedirectToPage("/ModuloAuth/Login");

            if (!ValidarNombre(out var err))
            { TempData["ErrorMessage"] = err; return RedirectToPage(new { pagina }); }

            var result = await _rolesApi.CrearRolAsync(
                token, Guid.NewGuid(), NombreRol.Trim(), PantallasSeleccionadas);

            TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] =
                result.Success
                    ? $"Rol '{NombreRol.Trim()}' creado exitosamente."
                    : result.ErrorMessage ?? "No se pudo crear el rol.";

            return RedirectToPage(new { pagina });
        }

        public async Task<IActionResult> OnPostEditarAsync(int pagina = 1)
        {
            var token = HttpContext.Session.GetString("JwtToken");
            if (string.IsNullOrEmpty(token))
                return RedirectToPage("/ModuloAuth/Login");

            if (RolID == Guid.Empty)
            { TempData["ErrorMessage"] = "Identificador de rol no valido."; return RedirectToPage(new { pagina }); }

            if (!ValidarNombre(out var err))
            { TempData["ErrorMessage"] = err; return RedirectToPage(new { pagina }); }

            var result = await _rolesApi.ActualizarRolAsync(
                token, RolID, NombreRol.Trim(), PantallasSeleccionadas);

            TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] =
                result.Success
                    ? $"Rol '{NombreRol.Trim()}' actualizado exitosamente."
                    : result.ErrorMessage ?? "No se pudo actualizar el rol.";

            return RedirectToPage(new { pagina });
        }

        public async Task<IActionResult> OnPostEliminarAsync(Guid rolId, int pagina = 1)
        {
            var token = HttpContext.Session.GetString("JwtToken");
            if (string.IsNullOrEmpty(token))
                return RedirectToPage("/ModuloAuth/Login");

            var result = await _rolesApi.EliminarRolAsync(token, rolId);

            TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] =
                result.Success
                    ? "Rol eliminado exitosamente."
                    : result.ErrorMessage ?? "No se pudo eliminar el rol.";

            // Volver a la p·gina anterior si el ˙ltimo registro de la p·gina fue eliminado
            var pageSize = _config.GetValue<int>("Pagination", 15);
            var paginaFin = pagina > 1 && result.Success ? pagina : pagina;
            return RedirectToPage(new { pagina = paginaFin });
        }

        private bool ValidarNombre(out string error)
        {
            error = string.Empty;
            if (string.IsNullOrWhiteSpace(NombreRol))
            { error = "El nombre del rol es obligatorio."; return false; }
            if (NombreRol.Trim().Length > 40)
            { error = "El nombre no puede superar 40 caracteres."; return false; }
            if (!Regex.IsMatch(NombreRol.Trim(), @"^[a-zA-Z·ÈÌÛ˙¡…Õ”⁄Ò—¸‹\s]+$"))
            { error = "El nombre solo puede contener letras y espacios."; return false; }
            return true;
        }
    }
}

