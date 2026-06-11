namespace MicroServicioFotografias.Entities
{
    // DTO de respuesta para el endpoint GET /usuario/fotografia/{id}.
    public class FotografiaResponse
    {
        // Email del usuario identificado en la consulta.
        public string Email { get; set; } = string.Empty;

        // Fotografía del usuario en formato Base64. Puede ser NULL si no tiene foto.
        public string? FotoBase64 { get; set; }
    }
}
