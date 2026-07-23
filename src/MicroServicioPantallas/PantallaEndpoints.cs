using System.Text.RegularExpressions;
using System.Text.Json;
using MicroServicioPantallas.Entities;
using MicroServicioPantallas.Services;
using MicroServicioPantallas.Security;
using Microsoft.AspNetCore.Mvc;

namespace MicroServicioPantallas
{
    public static class PantallaEndpoints
    {
        private static Guid ExtraerUsuarioId(string? token)
        {
            try
            {
                if (string.IsNullOrEmpty(token)) return Guid.Empty;
                var partes = token.Split('.');
                if (partes.Length < 2) return Guid.Empty;
                var payload = partes[1];
                var padding = payload.Length % 4;
                if (padding > 0) payload += new string('=', 4 - padding);
                var json = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(payload));
                var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("jti", out var jti) && Guid.TryParse(jti.GetString(), out var id))
                    return id;
            }
            catch { }
            return Guid.Empty;
        }

        public static void MapPantallaEndpoints(this IEndpointRouteBuilder routes)
        {
            var group = routes
                .MapGroup("/pantallas")
                .WithTags(nameof(Pantalla));

            group.MapGet("/", async (
                [FromServices] IPantallaService service,
                [FromServices] TokenValidator tokenValidator,
                [FromHeader(Name = "token")] string? token,
                int? pageNumber, int? pageSize, string? searchTerm,
                string? sortColumn, string? sortDirection, bool? incluirEliminados) =>
            {
                if (!await tokenValidator.EsValidoAsync(token))
                    return Results.Unauthorized();

                var (items, total) = await service.GetPaginadoAsync(
                    pageNumber ?? 1, pageSize ?? 10, searchTerm,
                    sortColumn ?? "",
                    string.IsNullOrWhiteSpace(sortDirection) ? "ASC" : sortDirection,
                    incluirEliminados ?? false);

                return Results.Ok(new { pageNumber = pageNumber ?? 1, pageSize = pageSize ?? 10, total, items });
            })
            .WithName("GetAllPantallas").WithOpenApi();

            group.MapGet("/{id}", async (
                [FromServices] IPantallaService service,
                [FromServices] TokenValidator tokenValidator,
                [FromHeader(Name = "token")] string? token,
                Guid id) =>
            {
                if (!await tokenValidator.EsValidoAsync(token))
                    return Results.Unauthorized();

                var p = await service.GetByIdAsync(id);
                return p is null ? Results.NotFound() : Results.Ok(p);
            })
            .WithName("GetPantallaById").WithOpenApi();

            group.MapPost("/", async (
                [FromServices] IPantallaService service,
                [FromServices] TokenValidator tokenValidator,
                [FromHeader(Name = "token")] string? token,
                [FromBody] Pantalla pantalla) =>
            {
                if (!await tokenValidator.EsValidoAsync(token))
                    return Results.Unauthorized();

                var errores = Validar(pantalla);
                if (errores.Count > 0) return Results.BadRequest(new { errores });

                var usuarioId = ExtraerUsuarioId(token);
                var creada = await service.CreateAsync(pantalla, usuarioId, token);
                if (creada is null) return Results.Problem("No se pudo crear la pantalla");

                return Results.Created($"/pantallas/{creada.PantallaID}", creada);
            })
            .WithName("CreatePantalla").WithOpenApi();

            group.MapPut("/{id}", async (
                [FromServices] IPantallaService service,
                [FromServices] TokenValidator tokenValidator,
                [FromHeader(Name = "token")] string? token,
                Guid id,
                [FromBody] Pantalla pantalla) =>
            {
                if (!await tokenValidator.EsValidoAsync(token))
                    return Results.Unauthorized();

                var errores = Validar(pantalla);
                if (errores.Count > 0) return Results.BadRequest(new { errores });

                var exists = await service.GetByIdAsync(id);
                if (exists is null) return Results.NotFound();

                pantalla.PantallaID = id;
                var usuarioId = ExtraerUsuarioId(token);
                await service.UpdateAsync(pantalla, usuarioId, token);

                var current = await service.GetByIdAsync(id) ?? pantalla;
                return Results.Ok(current);
            })
            .WithName("UpdatePantalla").WithOpenApi();

            group.MapDelete("/{id}", async (
                [FromServices] IPantallaService service,
                [FromServices] TokenValidator tokenValidator,
                [FromHeader(Name = "token")] string? token,
                Guid id) =>
            {
                if (!await tokenValidator.EsValidoAsync(token))
                    return Results.Unauthorized();

                var exists = await service.GetByIdAsync(id);
                if (exists is null) return Results.NotFound();

                var usuarioId = ExtraerUsuarioId(token);
                await service.LogicDeleteAsync(id, usuarioId, token);
                return Results.NoContent();
            })
            .WithName("DeletePantalla").WithOpenApi();
        }

        private static List<string> Validar(Pantalla pantalla)
        {
            var errores = new List<string>();
            var rx = @"^[a-zA-Z0-9 áéíóúÁÉÍÓÚñÑ]+$";

            if (string.IsNullOrWhiteSpace(pantalla.NombrePantalla))
                errores.Add("El nombre de la pantalla es requerido y no puede ser vacío ni espacios en blanco.");
            else
            {
                if (pantalla.NombrePantalla.Length > 100)
                    errores.Add("El nombre no puede tener más de 100 caracteres.");
                if (!Regex.IsMatch(pantalla.NombrePantalla, rx))
                    errores.Add("El nombre solo permite letras, números y espacios.");
            }

            if (string.IsNullOrWhiteSpace(pantalla.Descripcion))
                errores.Add("La descripción es requerida y no puede ser vacía ni espacios en blanco.");
            else
            {
                if (pantalla.Descripcion.Length > 500)
                    errores.Add("La descripción no puede tener más de 500 caracteres.");
                if (!Regex.IsMatch(pantalla.Descripcion, rx))
                    errores.Add("La descripción solo permite letras, números y espacios.");
            }

            if (string.IsNullOrWhiteSpace(pantalla.Ruta))
                errores.Add("La ruta de acceso es requerida y no puede ser vacía ni espacios en blanco.");
            else if (pantalla.Ruta.Length > 500)
                errores.Add("La ruta no puede tener más de 500 caracteres.");

            return errores;
        }
    }
}