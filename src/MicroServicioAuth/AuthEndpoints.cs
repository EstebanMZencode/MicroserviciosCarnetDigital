using MicroServicioAuth.Entities;
using MicroServicioAuth.Services;
using Microsoft.AspNetCore.Mvc;

namespace MicroServicioAuth
{
    /// <summary>
    /// Define y mapea los tres endpoints del MicroServicioAuth bajo el grupo <c>/auth</c>.
    /// Todos los parámetros se reciben por headers HTTP.
    /// </summary>
    public static class AuthEndpoints
    {
        public static void MapAuthEndpoints(this IEndpointRouteBuilder routes)
        {
            var group = routes
                .MapGroup("/api")
                .WithTags("Auth");

            group.MapPost("/login", MapLogin)
                .WithName("Login")
                .WithDescription("Valida credenciales, genera JWT y RefreshToken.")
                .WithOpenApi();

            group.MapPost("/refresh", MapRefresh)
                .WithName("Refresh")
                .WithDescription("Renueva JWT y RefreshToken si el token de refresco sigue vigente.")
                .WithOpenApi();

            group.MapGet("/validate", MapValidate)
                .WithName("Validate")
                .WithDescription("Valida si un JWT es válido y no ha expirado.")
                .WithOpenApi();
        }

        // ── POST /auth/login ──────────────────────────────────────────────────

        /// <summary>
        /// Valida las credenciales del usuario y retorna los tokens de sesión.
        /// Headers requeridos: <c>usuario</c> (email), <c>contrasena</c>, <c>tipo_usuario</c> (GUID).
        /// </summary>
        private static async Task<IResult> MapLogin(
            [FromHeader(Name = "usuario")] string? usuario,
            [FromHeader(Name = "contrasena")] string? contrasena,
            [FromHeader(Name = "tipo_usuario")] string? tipoUsuario,
            IAuthService authService)
        {
            // Validar que el email no esté vacío
            if (string.IsNullOrWhiteSpace(usuario))
                return Results.Json(new { message = "La identificación es obligatoria." }, statusCode: 400);

            // Validar que el email contenga el caracter @
            if (!usuario.Contains('@'))
                return Results.Json(new { message = "El usuario ingresado debe ser un correo electrónico." }, statusCode: 400);

            // Validar que la contraseña no esté vacía
            if (string.IsNullOrWhiteSpace(contrasena))
                return Results.Json(new { message = "La contraseña es obligatoria." }, statusCode: 400);

            // Validar que el tipo_usuario no esté vacío
            if (string.IsNullOrWhiteSpace(tipoUsuario))
                return Results.Json(new { message = "El tipo de usuario es obligatorio." }, statusCode: 400);

            var (data, statusCode, error) = await authService.LoginAsync(usuario, contrasena, tipoUsuario);

            return statusCode switch
            {
                201 => Results.Json(data, statusCode: 201),
                400 => Results.Json(new { message = error }, statusCode: 400),
                401 => Results.Json(new { message = error }, statusCode: 401),
                500 => Results.Json(new { message = error }, statusCode: 500),
                _ => Results.Json(new { message = error }, statusCode: statusCode)
            };
        }

        // ── POST /auth/refresh ────────────────────────────────────────────────

        /// <summary>
        /// Renueva los tokens si el RefreshToken sigue vigente en base de datos.
        /// Headers requeridos: <c>email</c>, <c>refresh_token</c>.
        /// </summary>
        private static async Task<IResult> MapRefresh(
            [FromHeader(Name = "email")] string? email,
            [FromHeader(Name = "refresh_token")] string? refreshToken,
            IAuthService authService)
        {
            // Validar que el email no esté vacío
            if (string.IsNullOrWhiteSpace(email))
                return Results.Json(new { message = "La identificación es obligatoria." }, statusCode: 400);

            // Validar que el refresh token no esté vacío
            if (string.IsNullOrWhiteSpace(refreshToken))
                return Results.Json(new { message = "El token es obligatorio." }, statusCode: 400);

            var (data, statusCode, error) = await authService.RefreshAsync(email, refreshToken);

            return statusCode switch
            {
                201 => Results.Json(data, statusCode: 201),
                400 => Results.Json(new { message = error }, statusCode: 400),
                401 => Results.Json(new { message = error }, statusCode: 401),
                500 => Results.Json(new { message = error }, statusCode: 500),
                _ => Results.Json(new { message = error }, statusCode: statusCode)
            };
        }

        // ── POST /auth/validate ───────────────────────────────────────────────

        /// <summary>
        /// Valida la firma, emisor, audiencia y vigencia del JWT recibido.
        /// Header requerido: <c>token</c> (JWT de acceso).
        /// Responde <c>200 true</c> si es válido o <c>401</c> si no lo es.
        /// </summary>
        private static IResult MapValidate(
            [FromHeader(Name = "token")] string? token,
            IAuthService authService)
        {
            // Validar que el token no esté vacío
            if (string.IsNullOrWhiteSpace(token))
                return Results.Json(new { message = "El token es obligatorio." }, statusCode: 400);

            var (isValid, statusCode, message) = authService.ValidateToken(token);

            return isValid
                ? Results.Ok("true")
                : Results.Json(new { message }, statusCode: 401);
        }
    }
}

