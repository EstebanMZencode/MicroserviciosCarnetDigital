using MicroServicioUsuarios.Entities;
using MicroServicioUsuarios.Services;
using Microsoft.AspNetCore.Mvc;

namespace MicroServicioUsuarios
{
    public static class UsuarioEndpoints
    {
        public static void MapUsuarioEndpoints(this IEndpointRouteBuilder routes)
        {
            var group = routes.MapGroup("/api/usuario");

            group.MapPost("/", MapCrear);
        }

        private static async Task<IResult> MapCrear(
            [FromBody] UsuarioCreateRequest request,
            [FromHeader(Name = "Authorization")] string? authorization,
            IUsuarioService usuarioService)
        {
            var token = ExtraerToken(authorization);
            if (string.IsNullOrWhiteSpace(token))
                return Results.Json(new { message = "No autorizado." }, statusCode: 401);

            // Validaciones básicas del body
            if (request.TipoIdentID == Guid.Empty)
                return Results.Json(new { message = "El tipo de identificación es obligatorio." }, statusCode: 400);

            if (string.IsNullOrWhiteSpace(request.Identificacion))
                return Results.Json(new { message = "La identificación es obligatoria." }, statusCode: 400);

            if (string.IsNullOrWhiteSpace(request.NombreCompleto))
                return Results.Json(new { message = "El nombre completo es obligatorio." }, statusCode: 400);

            if (request.EstadoID == Guid.Empty)
                return Results.Json(new { message = "El estado es obligatorio." }, statusCode: 400);

            if (string.IsNullOrWhiteSpace(request.Password))
                return Results.Json(new { message = "La contraseña es obligatoria." }, statusCode: 400);

            if (request.Emails == null || !request.Emails.Any())
                return Results.Json(new { message = "Debe proporcionar al menos un email." }, statusCode: 400);

            foreach (var email in request.Emails)
            {
                if (string.IsNullOrWhiteSpace(email.Email) || !email.Email.Contains('@'))
                    return Results.Json(new { message = $"El email '{email.Email}' no es válido." }, statusCode: 400);
                if (email.InstitucionID == Guid.Empty)
                    return Results.Json(new { message = "Cada email debe tener una institución asociada." }, statusCode: 400);
            }

            if (request.Instituciones == null || !request.Instituciones.Any())
                return Results.Json(new { message = "Debe proporcionar al menos una institución." }, statusCode: 400);

            foreach (var inst in request.Instituciones)
            {
                if (inst.InstitucionID == Guid.Empty)
                    return Results.Json(new { message = "El identificador de institución es obligatorio." }, statusCode: 400);
                if (inst.TipoUsuarioID == Guid.Empty)
                    return Results.Json(new { message = "El tipo de usuario es obligatorio." }, statusCode: 400);
                if (inst.RolID == Guid.Empty)
                    return Results.Json(new { message = "El rol es obligatorio." }, statusCode: 400);
                if (inst.FechaVencimientoCarnet <= DateTime.Now.Date)
                    return Results.Json(new { message = "La fecha de vencimiento debe ser futura." }, statusCode: 400);
            }

            var (data, statusCode, error) = await usuarioService.CrearUsuarioAsync(request, token);

            return statusCode switch
            {
                201 => Results.Created($"/api/usuario/{data!.UsuarioID}", data),
                400 => Results.Json(new { message = error }, statusCode: 400),
                401 => Results.Json(new { message = error }, statusCode: 401),
                404 => Results.Json(new { message = error }, statusCode: 404),
                500 => Results.Json(new { message = error }, statusCode: 500),
                _ => Results.Json(new { message = "Error inesperado." }, statusCode: 500)
            };
        }

        private static string? ExtraerToken(string? authorization)
            => authorization?.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) == true
                ? authorization[7..].Trim()
                : null;
    }
}