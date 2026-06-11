namespace MicroServicioAuth.Entities
{
    /// <summary>
    /// Cuerpo de la respuesta exitosa (HTTP 201) del endpoint <c>POST /auth/login</c>.
    /// Contiene los tokens de acceso y la información de sesión del usuario autenticado.
    /// </summary>
    public class LoginResponse
    {
        /// <summary>Fecha y hora UTC en que vence el JWT de acceso.</summary>
        public DateTime expires_in { get; set; }

        /// <summary>JWT firmado con HMAC-SHA256 para autenticar solicitudes.</summary>
        public string access_token { get; set; } = string.Empty;

        /// <summary>Token de refresco para renovar la sesión cuando el JWT expire.</summary>
        public string refresh_token { get; set; } = string.Empty;

        /// <summary>Email del usuario autenticado (identificador de sesión).</summary>
        public string usuarioID { get; set; } = string.Empty;

        /// <summary>RolID de la fila correspondiente en UsuariosXInstituciones.</summary>
        public Guid rol { get; set; }
    }
}
