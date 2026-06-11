namespace MicroServicioFotografias.Entities
{
    // Proyección de la tabla [Carnet_Identity_User].[Usuarios] que contiene
    // únicamente los campos necesarios para las operaciones de fotografía.
    public class UsuarioFoto
    {
        // PK de la tabla Usuarios. Se obtiene mediante JOIN con EmailXUsuarios.
        public Guid UsuarioID { get; set; }

        // Fotografía del usuario almacenada en Base64. Puede ser NULL.
        public string? FotoBase64 { get; set; }
    }
}
