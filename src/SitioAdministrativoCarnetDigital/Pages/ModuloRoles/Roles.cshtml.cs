using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SitioAdministrativoCarnetDigital.Services.MicroServicioRoles;

namespace SitioAdministrativoCarnetDigital.Pages.ModuloRoles
{
    public class RolesModel : PageModel
    {
        private readonly IRolesApiClient _rolesApi;
        private readonly IConfiguration _config;

        // Datos que usa la vista para renderizar la tabla y la paginación.
        public List<RolDto> Roles { get; private set; } = new();
        public int PaginaActual { get; private set; } = 1;
        public int TotalPaginas { get; private set; } = 1;
        public int TotalRegistros { get; private set; } = 0;

        // Mensajes de retroalimentación al usuario.
        // Se rellenan desde TempData (redirect) o directamente en el GET.
        public string? SuccessMessage { get; private set; }
        public string? ErrorMessage { get; private set; }

        public RolesModel(IRolesApiClient rolesApi, IConfiguration config)
        {
            _rolesApi = rolesApi;
            _config = config;
        }

        // Carga el listado paginado de roles y cualquier mensaje proveniente
        // de una acción anterior (crear, editar, eliminar).
        public async Task<IActionResult> OnGetAsync(int pagina = 1)
        {
            var token = HttpContext.Session.GetString("JwtToken");
            if (string.IsNullOrEmpty(token))
                return RedirectToPage("/ModuloAuth/Login");

            // Recoger mensajes dejados por redireccionamientos anteriores.
            SuccessMessage = TempData["SuccessMessage"]?.ToString();
            ErrorMessage = TempData["ErrorMessage"]?.ToString();

            var pageSize = _config.GetValue<int>("Pagination", 10);
            PaginaActual = pagina < 1 ? 1 : pagina;

            var result = await _rolesApi.GetRolesAsync(token, PaginaActual, pageSize);

            if (result.Success && result.Data is not null)
            {
                Roles = result.Data.Roles;
                TotalPaginas = result.Data.TotalPaginas < 1 ? 1 : result.Data.TotalPaginas;
                TotalRegistros = result.Data.TotalRegistros;
            }
            else
            {
                // El mensaje del body del microservicio ya viene en ErrorMessage.
                ErrorMessage = result.ErrorMessage ?? "No se pudieron cargar los roles.";
            }

            return Page();
        }

        // Soft-delete de un rol. El ID llega en el formulario oculto del modal
        // de confirmación. Redirige con TempData para mostrar el resultado.
        public async Task<IActionResult> OnPostDeleteAsync(Guid rolId)
        {
            var token = HttpContext.Session.GetString("JwtToken");
            if (string.IsNullOrEmpty(token))
                return RedirectToPage("/ModuloAuth/Login");

            var result = await _rolesApi.EliminarRolAsync(token, rolId);

            if (result.Success)
                TempData["SuccessMessage"] = "Rol eliminado exitosamente.";
            else
                TempData["ErrorMessage"] = result.ErrorMessage ?? "No se pudo eliminar el rol.";

            return RedirectToPage();
        }
    }
}
