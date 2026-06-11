using MicroServicioFotografias.Services;
using Microsoft.AspNetCore.Mvc;

namespace MicroServicioFotografias
{
    public static class FotografiasEndpoints
    {
        public static void MapFotografiasEndpoints(this IEndpointRouteBuilder routes)
        {
            var group = routes.MapGroup("/api/usuario/fotografia");

            group.MapPatch("/", MapActualizar);
            group.MapDelete("/", MapEliminar);
            group.MapGet("/{id}", MapObtener);
        }

        // ── PATCH /api/usuario/fotografia ─────────────────────────────────────

        private static async Task<IResult> MapActualizar(
            [FromHeader(Name = "identificador")] string? identificador,
            [FromHeader(Name = "fotografia")] string? fotografia,
            [FromHeader(Name = "Authorization")] string? authorization,
            IFotografiasService fotografiasService)
        {
            if (string.IsNullOrWhiteSpace(authorization))
                return Results.Json(new { message = "El token es obligatorio." }, statusCode: 400);

            var token = ExtraerToken(authorization);
            if (string.IsNullOrWhiteSpace(token))
                return Results.Json(new { message = "El token es obligatorio." }, statusCode: 400);

            if (string.IsNullOrWhiteSpace(identificador))
                return Results.Json(new { message = "El identificador del usuario es obligatorio." }, statusCode: 400);

            if (!identificador.Contains('@'))
                return Results.Json(new { message = "El identificador del usuario no es válido, debe ser un email." }, statusCode: 400);

            if (string.IsNullOrWhiteSpace(fotografia))
                return Results.Json(new { message = "La fotografía del usuario es obligatoria." }, statusCode: 400);

            var (statusCode, error) = await fotografiasService.ActualizarFotografiaAsync(
                identificador, fotografia, token);

            return statusCode == 204
                ? Results.NoContent()
                : Results.Json(new { message = error }, statusCode: statusCode);
        }

        // ── DELETE /api/usuario/fotografia ────────────────────────────────────

        private static async Task<IResult> MapEliminar(
            [FromHeader(Name = "identificador")] string? identificador,
            [FromHeader(Name = "Authorization")] string? authorization,
            IFotografiasService fotografiasService)
        {
            if (string.IsNullOrWhiteSpace(authorization))
                return Results.Json(new { message = "El token es obligatorio." }, statusCode: 400);

            var token = ExtraerToken(authorization);
            if (string.IsNullOrWhiteSpace(token))
                return Results.Json(new { message = "El token es obligatorio." }, statusCode: 400);

            if (string.IsNullOrWhiteSpace(identificador))
                return Results.Json(new { message = "El identificador del usuario es obligatorio." }, statusCode: 400);

            if (!identificador.Contains('@'))
                return Results.Json(new { message = "El identificador del usuario no es válido, debe ser un email." }, statusCode: 400);

            var (statusCode, error) = await fotografiasService.EliminarFotografiaAsync(
                identificador, token);

            return statusCode == 204
                ? Results.NoContent()
                : Results.Json(new { message = error }, statusCode: statusCode);
        }

        // ── GET /api/usuario/fotografia/{id} ──────────────────────────────────

        private static async Task<IResult> MapObtener(
            string id,
            [FromHeader(Name = "Authorization")] string? authorization,
            IFotografiasService fotografiasService)
        {
            if (string.IsNullOrWhiteSpace(authorization))
                return Results.Json(new { message = "El token es obligatorio." }, statusCode: 400);

            var token = ExtraerToken(authorization);
            if (string.IsNullOrWhiteSpace(token))
                return Results.Json(new { message = "El token es obligatorio." }, statusCode: 400);

            if (string.IsNullOrWhiteSpace(id))
                return Results.Json(new { message = "El identificador del usuario es obligatorio" }, statusCode: 400);

            var (data, statusCode, error) = await fotografiasService.ObtenerFotografiaAsync(id, token);

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

