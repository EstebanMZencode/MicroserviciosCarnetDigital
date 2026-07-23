using System.Text.RegularExpressions;
using System.Text.Json;
using MicroServicioParametros.Entities;
using MicroServicioParametros.Services;
using MicroServicioParametros.Security;
using Microsoft.AspNetCore.Mvc;

namespace MicroServicioParametros
{
    public static class ParametroEndpoints
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

        public static void MapParametroEndpoints(this IEndpointRouteBuilder routes)
        {
            var group = routes
                .MapGroup("/parametro")
                .WithTags(nameof(Parametro));

            group.MapGet("/", async (
                [FromServices] IParametroService service,
                [FromServices] TokenValidator tokenValidator,
                [FromHeader(Name = "token")] string? token,
                int? pageNumber, int? pageSize, string? searchTerm,
                string? sortColumn, string? sortDirection, bool? incluirEliminados) =>
            {
                if (!await tokenValidator.EsValidoAsync(token))
                    return Results.Unauthorized();

                var (items, total) = await service.GetPaginadoAsync(
                    pageNumber ?? 1, pageSize ?? 10, searchTerm,
                    string.IsNullOrWhiteSpace(sortColumn) ? "Identificador" : sortColumn,
                    string.IsNullOrWhiteSpace(sortDirection) ? "ASC" : sortDirection,
                    incluirEliminados ?? false);

                return Results.Ok(new { pageNumber = pageNumber ?? 1, pageSize = pageSize ?? 10, total, items });
            })
            .WithName("GetAllParametros").WithOpenApi();

            group.MapGet("/{id}", async (
                [FromServices] IParametroService service,
                [FromServices] TokenValidator tokenValidator,
                [FromHeader(Name = "token")] string? token,
                string id) =>
            {
                if (!await tokenValidator.EsValidoAsync(token))
                    return Results.Unauthorized();

                var p = await service.GetByIdAsync(id);
                return p is null ? Results.NotFound() : Results.Ok(p);
            })
            .WithName("GetParametroById").WithOpenApi();

            group.MapPost("/", async (
                [FromServices] IParametroService service,
                [FromServices] TokenValidator tokenValidator,
                [FromHeader(Name = "token")] string? token,
                [FromBody] Parametro parametro) =>
            {
                if (!await tokenValidator.EsValidoAsync(token))
                    return Results.Unauthorized();

                var errores = Validar(parametro);
                if (errores.Count > 0)
                    return Results.BadRequest(new { errores });

                var exists = await service.GetByIdAsync(parametro.Identificador);
                if (exists is not null)
                    return Results.Conflict(new { message = $"Ya existe un parámetro con el identificador '{parametro.Identificador}'." });

                var usuarioId = ExtraerUsuarioId(token);
                var creado = await service.CreateAsync(parametro, usuarioId, token);
                if (creado is null)
                    return Results.Problem("No se pudo crear el parámetro");

                return Results.Created($"/parametro/{creado.Identificador}", creado);
            })
            .WithName("CreateParametro").WithOpenApi();

            group.MapPut("/{id}", async (
                [FromServices] IParametroService service,
                [FromServices] TokenValidator tokenValidator,
                [FromHeader(Name = "token")] string? token,
                string id,
                [FromBody] Parametro parametro) =>
            {
                if (!await tokenValidator.EsValidoAsync(token))
                    return Results.Unauthorized();

                if (!string.Equals(id, parametro.Identificador, StringComparison.Ordinal))
                    return Results.BadRequest(new { message = "El identificador de la ruta y el del cuerpo no coinciden" });

                var errores = Validar(parametro);
                if (errores.Count > 0)
                    return Results.BadRequest(new { errores });

                var exists = await service.GetByIdAsync(id);
                if (exists is null)
                    return Results.NotFound();

                var usuarioId = ExtraerUsuarioId(token);
                await service.UpdateAsync(parametro, usuarioId, token);

                var current = await service.GetByIdAsync(id) ?? parametro;
                return Results.Ok(current);
            })
            .WithName("UpdateParametro").WithOpenApi();

            group.MapDelete("/{id}", async (
                [FromServices] IParametroService service,
                [FromServices] TokenValidator tokenValidator,
                [FromHeader(Name = "token")] string? token,
                string id) =>
            {
                if (!await tokenValidator.EsValidoAsync(token))
                    return Results.Unauthorized();

                var exists = await service.GetByIdAsync(id);
                if (exists is null)
                    return Results.NotFound();

                var usuarioId = ExtraerUsuarioId(token);
                await service.LogicDeleteAsync(id, usuarioId, token);
                return Results.NoContent();
            })
            .WithName("DeleteParametro").WithOpenApi();
        }

        private static List<string> Validar(Parametro parametro)
        {
            var errores = new List<string>();

            if (string.IsNullOrWhiteSpace(parametro.Identificador))
                errores.Add("El identificador del parámetro es requerido y no puede ser vacío ni espacios en blanco.");
            else
            {
                if (parametro.Identificador.Length > 10)
                    errores.Add("El identificador no puede tener más de 10 caracteres.");
                if (!Regex.IsMatch(parametro.Identificador, "^[A-Z]+$"))
                    errores.Add("El identificador solo permite letras en mayúscula (A-Z).");
            }

            if (string.IsNullOrWhiteSpace(parametro.Valor))
                errores.Add("El valor del parámetro es requerido y no puede ser vacío ni espacios en blanco.");
            else if (parametro.Valor.Length > 500)
                errores.Add("El valor no puede tener más de 500 caracteres.");

            return errores;
        }
    }
}