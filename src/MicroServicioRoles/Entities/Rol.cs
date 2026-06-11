namespace MicroServicioRoles.Entities
{
    // Mapea la tabla [Carnet_Access_User].[Roles].
    public class Rol
    {
        // PK de la tabla Roles. UNIQUEIDENTIFIER generado por NEWSEQUENTIALID().
        public Guid RolID { get; set; }

        // Nombre único del rol. Solo permite letras, números y espacios (máx. 50 chars).
        public string NombreRol { get; set; } = string.Empty;

        public DateTime FechaCreacion { get; set; }

        public DateTime FechaModificacion { get; set; }

        // 1 = activo, 0 = inactivo (soft delete, triggers bloquean DELETE físico).
        public bool Estado { get; set; }
    }
}
