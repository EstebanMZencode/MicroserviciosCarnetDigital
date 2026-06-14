namespace MicroServicioRoles.Entities
{
    // Mapea la tabla [Carnet_Access_User].[PantallasXRoles].
    // Relaciona un Rol con las Pantallas que tiene asignadas.
    public class PantallaXRol
    {
        // PK de la tabla PantallasXRoles.
        public Guid PXRID { get; set; }

        // FK a Roles.RolID.
        public Guid RolID { get; set; }

        // FK a Pantallas.PantallaID.
        public Guid PantallaID { get; set; }

        // 1 = activo, 0 = inactivo (soft delete).
        public bool Estado { get; set; }
    }
}
