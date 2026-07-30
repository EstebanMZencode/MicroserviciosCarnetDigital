using System.Text.Json.Serialization;

namespace SitioAdministrativoCarnetDigital.Services.MicroServicioAutoregistro
{
    // Body del POST /autoregistro — refleja exactamente la entidad UsuarioRegistro del microservicio.
    public class UsuarioRegistroRequest
    {
        [JsonPropertyName("tipoIdentID")]
        public Guid TipoIdentID { get; set; }

        [JsonPropertyName("identificacion")]
        public string Identificacion { get; set; } = string.Empty;

        [JsonPropertyName("nombreCompleto")]
        public string NombreCompleto { get; set; } = string.Empty;

        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("contrasena")]
        public string Contrasena { get; set; } = string.Empty;

        [JsonPropertyName("institucionID")]
        public Guid InstitucionID { get; set; }

        [JsonPropertyName("tipoUsuarioID")]
        public Guid TipoUsuarioID { get; set; }

        [JsonPropertyName("rolID")]
        public Guid RolID { get; set; }

        // Un año desde hoy — calculado en el PageModel, nunca mostrado al usuario.
        [JsonPropertyName("fechaVencimientoCarnet")]
        public DateTime FechaVencimientoCarnet { get; set; }

        // Solo para Estudiantes; vacío para Funcionarios.
        [JsonPropertyName("carrerasIDs")]
        public List<Guid> CarrerasIDs { get; set; } = new();

        // Solo para Funcionarios; vacío para Estudiantes.
        [JsonPropertyName("areasTrabajoIDs")]
        public List<Guid> AreasTrabajoIDs { get; set; } = new();

        // Opcional para ambos tipos.
        [JsonPropertyName("telefonos")]
        public List<string> Telefonos { get; set; } = new();
    }

    // Respuesta del POST /autoregistro (201) y del GET /autoregistro/confirmar (200).
    public class AutoregistroResponseDto
    {
        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;
    }
}
