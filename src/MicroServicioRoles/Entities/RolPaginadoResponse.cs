namespace MicroServicioRoles.Entities
{
    // DTO de respuesta paginada para el endpoint GET /api/rol.
    public class RolPaginadoResponse
    {
        // Total de roles activos en base de datos (sin paginar).
        public int TotalRegistros { get; set; }

        // Número de página actual (base 1).
        public int Pagina { get; set; }

        // Cantidad de registros por página.
        public int Tamano { get; set; }

        // Roles de la página actual con sus pantallas.
        public List<RolResponse> Roles { get; set; } = new();
    }
}
