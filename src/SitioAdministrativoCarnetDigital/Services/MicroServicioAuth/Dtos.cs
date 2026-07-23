using System.Text.Json.Serialization;

namespace SitioAdministrativoCarnetDigital.Services.MicroServicioAuth
{
    // Respuesta real del endpoint POST /api/auth/login de MicroServicioAuth.
    // Campos verificados contra el transcript del microservicio:
    //   - usuarioID  → email del usuario autenticado
    //   - expires_in → DateTime UTC de expiración del JWT (no int)
    //   - rol        → GUID del rol asignado
    public class LoginResponseDto
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = string.Empty;

        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; } = string.Empty;

        // El microservicio devuelve "usuarioID", no "email".
        [JsonPropertyName("usuarioID")]
        public string UsuarioID { get; set; } = string.Empty;

        // El microservicio devuelve un DateTime UTC, no un int de minutos.
        [JsonPropertyName("expires_in")]
        public DateTime ExpiresIn { get; set; }

        [JsonPropertyName("rol")]
        public Guid Rol { get; set; }
    }
}

