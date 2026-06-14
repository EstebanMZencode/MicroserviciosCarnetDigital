using System.Text.RegularExpressions;
using MicroServicioRoles.Entities;
using MicroServicioRoles.Services;
using Microsoft.AspNetCore.Mvc;

namespace MicroServicioRoles
{
    public static class RolesEndpoints
    {
        public static void MapRolesEndpoints(this IEndpointRouteBuilder routes)
        {
            var group = routes.MapGroup("/api/rol");

            group.MapPost("/", MapCrear);
            group.MapPut("/", MapModificar);
            group.MapDelete("/", MapEliminar);
            group.MapGet("/", MapObtenerTodos);
            group.MapGet("/{id}", MapObtenerPorId);
        }

        // ── POST /api/rol ─────────────────────────────────────────────────────

        private static async Task<IResult> MapCrear(
            [FromHeader(Name = "identificador")] string? identificador,
            [FromHeader(Name = "nombre_rol")] string? nombreRol,
            [FromHeader(Name = "Authorization")] string? authorization,
            [FromBody] List<Guid>? pantallas,
            IRolesService rolesService,
            HttpContext ctx)
        {
            var token = ExtraerToken(authorization);
            if (string.IsNullOrWhiteSpace(token))
                return Results.Json(new { message = "No autorizado." }, statusCode: 401);

            if (string.IsNullOrWhiteSpace(identificador))
                return Results.Json(new { message = "El identificador es obligatorio." }, statusCode: 400);

            if (identificador.Contains('\t'))
                return Results.Json(new { message = "El identificador y/o el nombre no son válidos." }, statusCode: 400);

            if (!Guid.TryParse(identificador, out var rolId))
                return Results.Json(new { message = "El identificador no tiene un formato válido." }, statusCode: 400);

            if (string.IsNullOrWhiteSpace(nombreRol))
                return Results.Json(new { message = "El nombre del rol es obligatorio." }, statusCode: 400);

            if (nombreRol.Contains('\t'))
                return Results.Json(new { message = "El identificador y/o el nombre no son válidos." }, statusCode: 400);

            if (!Regex.IsMatch(nombreRol, @"^[a-zA-Z0-9 ]+$"))
                return Results.Json(new { message = "Solo se permiten letras, números y espacios en el nombre del rol." }, statusCode: 400);

            if (pantallas is null || !pantallas.Any())
                return Results.Json(new { message = "Debe asignar al menos una pantalla al rol." }, statusCode: 400);

            var (data, statusCode, error) = await rolesService.CrearRolAsync(rolId, nombreRol, pantallas, token);

            if (statusCode != 201)
                return Results.Json(new { message = error }, statusCode: statusCode);

            var locationUrl = $"{ctx.Request.Scheme}://{ctx.Request.Host}/api/rol/{rolId}";
            return Results.Created(locationUrl, data);
        }

        // ── PUT /api/rol ──────────────────────────────────────────────────────

        private static async Task<IResult> MapModificar(
            [FromHeader(Name = "identificador")] string? identificador,
            [FromHeader(Name = "nombre_rol")] string? nombreRol,
            [FromHeader(Name = "Authorization")] string? authorization,
            [FromBody] List<Guid>? pantallas,
            IRolesService rolesService)
        {
            var token = ExtraerToken(authorization);
            if (string.IsNullOrWhiteSpace(token))
                return Results.Json(new { message = "No autorizado." }, statusCode: 401);

            if (string.IsNullOrWhiteSpace(identificador))
                return Results.Json(new { message = "El identificador es obligatorio." }, statusCode: 400);

            if (identificador.Contains('\t'))
                return Results.Json(new { message = "El identificador y/o el nombre no son válidos." }, statusCode: 400);

            if (!Guid.TryParse(identificador, out var rolId))
                return Results.Json(new { message = "El identificador no tiene un formato válido." }, statusCode: 400);

            if (string.IsNullOrWhiteSpace(nombreRol))
                return Results.Json(new { message = "El nombre del rol es obligatorio." }, statusCode: 400);

            if (nombreRol.Contains('\t'))
                return Results.Json(new { message = "El identificador y/o el nombre no son válidos." }, statusCode: 400);

            if (!Regex.IsMatch(nombreRol, @"^[a-zA-Z0-9 ]+$"))
                return Results.Json(new { message = "Solo se permiten letras, números y espacios en el nombre del rol." }, statusCode: 400);

            if (pantallas is null || !pantallas.Any())
                return Results.Json(new { message = "Debe asignar al menos una pantalla al rol." }, statusCode: 400);

            var (statusCode, error) = await rolesService.ModificarRolAsync(rolId, nombreRol, pantallas, token);

            return statusCode == 204
                ? Results.NoContent()
                : Results.Json(new { message = error }, statusCode: statusCode);
        }

        // ── DELETE /api/rol ───────────────────────────────────────────────────

        private static async Task<IResult> MapEliminar(
            [FromHeader(Name = "identificador")] string? identificador,
            [FromHeader(Name = "Authorization")] string? authorization,
            IRolesService rolesService)
        {
            var token = ExtraerToken(authorization);
            if (string.IsNullOrWhiteSpace(token))
                return Results.Json(new { message = "No autorizado." }, statusCode: 401);

            if (string.IsNullOrWhiteSpace(identificador))
                return Results.Json(new { message = "El identificador es obligatorio." }, statusCode: 400);

            if (identificador.Contains('\t'))
                return Results.Json(new { message = "El identificador del rol no es válido." }, statusCode: 400);

            if (!Guid.TryParse(identificador, out var rolId))
                return Results.Json(new { message = "El identificador no tiene un formato válido." }, statusCode: 400);

            var (statusCode, error) = await rolesService.EliminarRolAsync(rolId, token);

            return statusCode == 204
                ? Results.NoContent()
                : Results.Json(new { message = error }, statusCode: statusCode);
        }

        // ── GET /api/rol ──────────────────────────────────────────────────────
        // pagina y tamano se leen desde QueryString para evitar conflictos
        // con la resolución DI en Minimal APIs al mezclar [FromQuery] con defaults.

        private static async Task<IResult> MapObtenerTodos(
            [FromHeader(Name = "Authorization")] string? authorization,
            HttpContext ctx,
            IRolesService rolesService)
        {
            var token = ExtraerToken(authorization);
            if (string.IsNullOrWhiteSpace(token))
                return Results.Json(new { message = "No autorizado." }, statusCode: 401);

            var pagina = int.TryParse(ctx.Request.Query["pagina"], out var p) && p > 0 ? p : 1;
            var tamano = int.TryParse(ctx.Request.Query["tamano"], out var t) && t > 0 ? t : 10;

            var (data, statusCode, error) = await rolesService.ObtenerRolesAsync(pagina, tamano, token);

            return statusCode == 200
                ? Results.Ok(data)
                : Results.Json(new { message = error }, statusCode: statusCode);
        }

        // ── GET /api/rol/{id} ─────────────────────────────────────────────────

        private static async Task<IResult> MapObtenerPorId(
            string id,
            [FromHeader(Name = "Authorization")] string? authorization,
            IRolesService rolesService)
        {
            var token = ExtraerToken(authorization);
            if (string.IsNullOrWhiteSpace(token))
                return Results.Json(new { message = "No autorizado." }, statusCode: 401);

            if (string.IsNullOrWhiteSpace(id))
                return Results.Json(new { message = "El identificador es obligatorio." }, statusCode: 400);

            if (id.Contains('\t'))
                return Results.Json(new { message = "El identificador del rol no es válido." }, statusCode: 400);

            if (!Guid.TryParse(id, out var rolId))
                return Results.Json(new { message = "El identificador no tiene un formato válido." }, statusCode: 400);

            var (data, statusCode, error) = await rolesService.ObtenerRolPorIdAsync(rolId, token);

            return statusCode == 200
                ? Results.Ok(data)
                : Results.Json(new { message = error }, statusCode: statusCode);
        }

        // Extrae el JWT quitando el prefijo "Bearer " del header Authorization.
        private static string? ExtraerToken(string? authorization)
            => authorization?.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) == true
                ? authorization[7..].Trim()
                : null;
    }
}
