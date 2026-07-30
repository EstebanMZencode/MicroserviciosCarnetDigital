using System.Text.Json.Serialization;

namespace SitioAdministrativoCarnetDigital.Services.MicroServicioFotografias
{
    // Respuesta del GET /api/usuario/fotografia/{correo}
    public class FotografiaDto
    {
        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        // Base64 de la imagen. Null o vacío si el usuario no tiene foto registrada.
        [JsonPropertyName("fotoBase64")]
        public string? FotoBase64 { get; set; }
    }

    // Body del PATCH /api/usuario/fotografia
    public class ActualizarFotografiaRequest
    {
        [JsonPropertyName("identificador")]
        public string Identificador { get; set; } = string.Empty;

        [JsonPropertyName("fotografia")]
        public string Fotografia { get; set; } = string.Empty;
    }
}
