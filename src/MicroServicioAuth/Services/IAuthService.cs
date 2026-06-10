using MicroServicioAuth.Entities;

namespace MicroServicioAuth.Services
{
    /// <summary>
    /// Contrato del servicio de autenticación.
    /// Actúa como mediador entre <c>AuthEndpoints</c> y <c>AuthRepository</c>.
    /// Cada método corresponde a uno de los tres endpoints del microservicio.
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Valida las credenciales del usuario contra la base de datos,
        /// genera el JWT y el RefreshToken, los persiste y notifica a Bitácora.
        /// </summary>
        /// <param name="email">Email del usuario recibido en el header <c>usuario</c>.</param>
        /// <param name="password">Contraseña en texto plano recibida en el header <c>contrasena</c>.</param>
        /// <param name="tipoUsuario">GUID del TipoUsuario recibido en el header <c>tipo_usuario</c>.</param>
        /// <returns>Tupla con la respuesta exitosa, el código HTTP y el mensaje de error si aplica.</returns>
        Task<(LoginResponse? data, int statusCode, string? error)> LoginAsync(
            string email, string password, string tipoUsuario);

        /// <summary>
        /// Valida que el RefreshToken siga vigente en base de datos,
        /// genera nuevos tokens, actualiza la tabla RefreshToken y notifica a Bitácora.
        /// </summary>
        /// <param name="email">Email del usuario recibido en el header <c>email</c>.</param>
        /// <param name="refreshToken">Token de refresco recibido en el header <c>refresh_token</c>.</param>
        /// <returns>Tupla con la respuesta exitosa, el código HTTP y el mensaje de error si aplica.</returns>
        Task<(RefreshResponse? data, int statusCode, string? error)> RefreshAsync(
            string email, string refreshToken);

        /// <summary>
        /// Valida la firma, el emisor, la audiencia y la vigencia del JWT recibido.
        /// No realiza consultas a base de datos; la validación es completamente en memoria.
        /// </summary>
        /// <param name="token">JWT a validar, recibido en el header <c>token</c>.</param>
        /// <returns>Tupla con indicador de validez, código HTTP y mensaje de respuesta.</returns>
        (bool isValid, int statusCode, string message) ValidateToken(string token);
    }
}

