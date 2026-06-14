namespace MicroServicioAuth.Entities
{
    /// <summary>
    /// Cuerpo de la respuesta exitosa (HTTP 201) del endpoint <c>POST /auth/refresh</c>.
    /// Contiene los nuevos tokens generados al refrescar una sesión válida.
    /// </summary>
    public class RefreshResponse
    {
        /// <summary>Fecha y hora UTC en que vence el nuevo JWT de acceso.</summary>
        public DateTime expires_in { get; set; }

        /// <summary>Nuevo JWT firmado con HMAC-SHA256.</summary>
        public string access_token { get; set; } = string.Empty;

        /// <summary>Nuevo token de refresco. Reemplaza al anterior en base de datos.</summary>
        public string refresh_token { get; set; } = string.Empty;
    }
}
