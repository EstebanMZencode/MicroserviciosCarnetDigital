namespace MicroServicioAuth.Entities
{
    /// <summary>
    /// Mapea la tabla <c>[Carnet_Identity_User].[RefreshToken]</c>.
    /// Se usa para validar y persistir tokens de refresco de sesión.
    /// </summary>
    public class RefreshTokenRecord
    {
        /// <summary>PK y FK a Login. Email del usuario propietario del token.</summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>Token de refresco generado criptográficamente (Base64, 64 bytes).</summary>
        public string TokenRefresh { get; set; } = string.Empty;

        /// <summary>Fecha y hora UTC de vencimiento del token de refresco.</summary>
        public DateTime FechaExpiracion { get; set; }
    }
}
