using MicroServicioActualizarEstadoUsuario.Services;
using Microsoft.AspNetCore.Mvc;

namespace MicroServicioActualizarEstadoUsuario
{
    public static class EstadoUsuarioEndpoints
    {
        public static void MapEstadoUsuarioEndpoints(this IEndpointRouteBuilder routes)
        {
            var group = routes.MapGroup("/api/usuarios/estado");

            group.MapPatch("/", MapCambiarEstado);
        }

        // PATCH /api/usuarios/estado
        private static async Task<IResult> MapCambiarEstado(
            [FromHeader(Name = "EmailUsuario")] string? emailUsuario,
            [FromHeader(Name = "EstadoID")] string? estadoID,
            [FromHeader(Name = "Authorization")] string? authorization,
            IEstadoUsuarioService estadoService)
        {
            // Validación JWT
            var token = ExtraerToken(authorization);
            if (string.IsNullOrWhiteSpace(token))
                return Results.Json(new { message = "No autorizado." }, statusCode: 401);

            // Validación Headers
            if (string.IsNullOrWhiteSpace(emailUsuario))
                return Results.Json(new { message = "El header EmailUsuario es obligatorio." }, statusCode: 400);

            if (string.IsNullOrWhiteSpace(estadoID))
                return Results.Json(new { message = "El header EstadoID es obligatorio." }, statusCode: 400);

            if (!Guid.TryParse(estadoID, out var estadoGuid))
                return Results.Json(new { message = "El EstadoID no tiene un formato válido." }, statusCode: 400);

            // Update
            var (data, statusCode, error) = await estadoService.UpdateEstadoUsuarioAsync(
                emailUsuario, estadoGuid, token);

            return statusCode switch
            {
                200 => Results.Ok(new { message = "Estado actualizado correctamente.", data }),
                401 => Results.Json(new { message = error }, statusCode: 401),
                404 => Results.Json(new { message = error }, statusCode: 404),
                500 => Results.Json(new { message = error }, statusCode: 500),
                _ => Results.Json(new { message = "Error inesperado." }, statusCode: 500)
            };
        }

        // Extrae el JWT 
        private static string? ExtraerToken(string? authorization)
            => authorization?.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) == true
                ? authorization[7..].Trim()
                : null;
    }
}