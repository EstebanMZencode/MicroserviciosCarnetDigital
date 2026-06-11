using MicroServicioRoles.Entities;

namespace MicroServicioRoles.Services
{
    // Contrato del servicio de roles.
    // Cada método valida el JWT, ejecuta la operación y registra en bitácora.
    public interface IRolesService
    {
        // Crea un rol con sus pantallas asignadas.
        Task<(RolResponse? data, int statusCode, string? error)> CrearRolAsync(
            Guid rolId, string nombreRol, List<Guid> pantallas, string token);

        // Actualiza el nombre del rol y sincroniza sus pantallas asignadas.
        Task<(int statusCode, string? error)> ModificarRolAsync(
            Guid rolId, string nombreRol, List<Guid> pantallas, string token);

        // Soft delete del rol y de todos sus registros en PantallasXRoles.
        Task<(int statusCode, string? error)> EliminarRolAsync(
            Guid rolId, string token);

        // Recupera todos los roles activos paginados con sus pantallas.
        Task<(RolPaginadoResponse? data, int statusCode, string? error)> ObtenerRolesAsync(
            int pagina, int tamano, string token);

        // Recupera un rol activo específico con sus pantallas asignadas.
        Task<(RolResponse? data, int statusCode, string? error)> ObtenerRolPorIdAsync(
            Guid rolId, string token);
    }
}

