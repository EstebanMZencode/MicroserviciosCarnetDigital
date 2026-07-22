namespace SitioAdministrativoCarnetDigital.Services.MicroServicioRoles
{
    public interface IRolesApiClient
    {
        // Obtiene el listado paginado de roles.
        // GET {RolesUrl}/api/rol?pagina=&tamano=
        Task<ApiResult<RolPaginadoDto>> GetRolesAsync(string token, int pagina = 1, int tamano = 10);

        // Obtiene un rol por su ID con las pantallas que tiene asignadas.
        // GET {RolesUrl}/api/rol/{id}
        Task<ApiResult<RolDto>> GetRolAsync(string token, Guid rolId);

        // Crea un nuevo rol. El identificador (GUID) lo genera el cliente.
        // POST {RolesUrl}/api/rol — Headers: identificador, nombre_rol, Authorization Bearer
        // Body: ["pantallaGuid1","pantallaGuid2",...]
        Task<ApiResult<bool>> CrearRolAsync(string token, Guid rolId, string nombreRol, List<Guid> pantallas);

        // Actualiza nombre y pantallas de un rol existente.
        // PUT {RolesUrl}/api/rol — mismos headers y body que el POST
        Task<ApiResult<bool>> ActualizarRolAsync(string token, Guid rolId, string nombreRol, List<Guid> pantallas);

        // Elimina (soft delete) un rol.
        // DELETE {RolesUrl}/api/rol — Header: identificador
        Task<ApiResult<bool>> EliminarRolAsync(string token, Guid rolId);

        // Obtiene el catálogo completo de pantallas del sistema.
        // GET {PantallasUrl}/pantallas — Header: token (no Bearer)
        // Se hace desde aquí para evitar registrar IPantallasApiClient en Program.cs.
        Task<ApiResult<List<PantallaDto>>> GetPantallasAsync(string token, int pageSize = 200);
    }
}
