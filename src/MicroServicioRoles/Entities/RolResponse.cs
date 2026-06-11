namespace MicroServicioRoles.Entities
{
    // DTO de respuesta para un rol con sus pantallas asignadas activas.
    // Usado en POST (201), GET por ID (200) y GET paginado (200).
    public class RolResponse
    {
        // Identificador único del rol.
        public Guid RolID { get; set; }

        // Nombre del rol.
        public string NombreRol { get; set; } = string.Empty;

        // Lista de PantallaID asignadas y activas para este rol.
        public List<Guid> Pantallas { get; set; } = new();
    }
}
