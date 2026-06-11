namespace MicroServicioRoles.Entities
{
    // Mapea la tabla [Carnet_Access_User].[Pantallas].
    public class Pantalla
    {
        // PK de la tabla Pantallas.
        public Guid PantallaID { get; set; }

        // Nombre único de la pantalla (máx. 100 chars, solo alfanumérico y espacios).
        public string NombrePantalla { get; set; } = string.Empty;

        // Descripción de la pantalla (máx. 500 chars).
        public string Descripcion { get; set; } = string.Empty;

        // Ruta de la pantalla en el frontend (máx. 500 chars, único).
        public string Ruta { get; set; } = string.Empty;

        // 1 = activo, 0 = inactivo (soft delete).
        public bool Estado { get; set; }
    }
}
