namespace MicroServicioFotografias.Entities
{
    // DTO para el request PATCH /usuario/fotografia.
    // Recibe email y foto Base64 en el body JSON.
    public class ActualizarFotografiaRequest
    {
        // Email del usuario (identificador).
        public string Identificador { get; set; } = string.Empty;

        // Fotografía en formato Base64.
        public string Fotografia { get; set; } = string.Empty;
    }
}
