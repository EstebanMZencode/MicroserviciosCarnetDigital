using System.Text.RegularExpressions;
using MicroServicioParametros.Entities;
using MicroServicioParametros.Services;
using Microsoft.AspNetCore.Mvc;

namespace MicroServicioParametros
{
    public static class ParametroEndpoints
    {
        public static void MapParametroEndpoints(this IEndpointRouteBuilder routes)
        {
            var group = routes
                .MapGroup("/parametro")
                .WithTags(nameof(Parametro));

            // GET /parametro  -> Obtener todos
            group.MapGet("/", async ([FromServices] IParametroService service) =>
            {
                return Results.Ok(await service.GetAllAsync());
            })
            .WithName("GetAllParametros")
            .WithOpenApi();

            // GET /parametro/{id} -> Obtener por llave primaria
            group.MapGet("/{id}", async ([FromServices] IParametroService service, string id) =>
            {
                var p = await service.GetByIdAsync(id);
                return p is null ? Results.NotFound() : Results.Ok(p);
            })
            .WithName("GetParametroById")
            .WithOpenApi();

            // POST /parametro -> Crear
            group.MapPost("/", async ([FromServices] IParametroService service, [FromBody] Parametro parametro) =>
            {
                var errores = Validar(parametro);
                if (errores.Count > 0)
                {
                    return Results.BadRequest(new { errores });
                }

                // 409 si ya existe (la llave primaria es el identificador)
                var exists = await service.GetByIdAsync(parametro.Identificador);
                if (exists is not null)
                {
                    return Results.Conflict(new { message = $"Ya existe un parámetro con el identificador '{parametro.Identificador}'." });
                }

                var rows = await service.CreateAsync(parametro);
                if (rows <= 0)
                {
                    return Results.Problem("No se pudo crear el parámetro");
                }

                var created = await service.GetByIdAsync(parametro.Identificador) ?? parametro;
                return Results.Created($"/parametro/{created.Identificador}", created);
            })
            .WithName("CreateParametro")
            .WithOpenApi();

            // PUT /parametro/{id} -> Modificar
            group.MapPut("/{id}", async ([FromServices] IParametroService service, string id, [FromBody] Parametro parametro) =>
            {
                if (!string.Equals(id, parametro.Identificador, StringComparison.Ordinal))
                {
                    return Results.BadRequest(new { message = "El identificador de la ruta y el del cuerpo no coinciden" });
                }

                var errores = Validar(parametro);
                if (errores.Count > 0)
                {
                    return Results.BadRequest(new { errores });
                }

                var exists = await service.GetByIdAsync(id);
                if (exists is null)
                {
                    return Results.NotFound();
                }

                var updated = await service.UpdateAsync(parametro);
                if (updated <= 0)
                {
                    return Results.Problem("No se pudo modificar el parámetro");
                }

                var current = await service.GetByIdAsync(id) ?? parametro;
                return Results.Ok(current);
            })
            .WithName("UpdateParametro")
            .WithOpenApi();

            // DELETE /parametro/{id} -> Eliminar
            group.MapDelete("/{id}", async ([FromServices] IParametroService service, string id) =>
            {
                var exists = await service.GetByIdAsync(id);
                if (exists is null)
                {
                    return Results.NotFound();
                }

                var deleted = await service.DeleteAsync(id);
                return deleted > 0 ? Results.NoContent() : Results.Problem("No se pudo eliminar el parámetro");
            })
            .WithName("DeleteParametro")
            .WithOpenApi();
        }

        // Validaciones según criterios de aceptación de SRV15
        private static List<string> Validar(Parametro parametro)
        {
            var errores = new List<string>();

            // Requeridos, no vacíos ni solo espacios en blanco
            if (string.IsNullOrWhiteSpace(parametro.Identificador))
            {
                errores.Add("El identificador del parámetro es requerido y no puede ser vacío ni espacios en blanco.");
            }
            else
            {
                // Máximo 10 caracteres y solo letras en mayúscula
                if (parametro.Identificador.Length > 10)
                {
                    errores.Add("El identificador no puede tener más de 10 caracteres.");
                }
                if (!Regex.IsMatch(parametro.Identificador, "^[A-Z]+$"))
                {
                    errores.Add("El identificador solo permite letras en mayúscula (A-Z).");
                }
            }

            if (string.IsNullOrWhiteSpace(parametro.Valor))
            {
                errores.Add("El valor del parámetro es requerido y no puede ser vacío ni espacios en blanco.");
            }
            else if (parametro.Valor.Length > 500)
            {
                errores.Add("El valor no puede tener más de 500 caracteres.");
            }

            return errores;
        }
    }
}
