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

            // GET /parametro -> Obtener paginado
            // Parámetros opcionales por query string, ej:
            //   /parametro?pageNumber=1&pageSize=10&searchTerm=TOKEN&sortColumn=Identificador&sortDirection=ASC&incluirEliminados=false
            group.MapGet("/", async (
                [FromServices] IParametroService service,
                int? pageNumber,
                int? pageSize,
                string? searchTerm,
                string? sortColumn,
                string? sortDirection,
                bool? incluirEliminados) =>
            {
                var (items, total) = await service.GetPaginadoAsync(
                    pageNumber ?? 1,
                    pageSize ?? 10,
                    searchTerm,
                    string.IsNullOrWhiteSpace(sortColumn) ? "Identificador" : sortColumn,
                    string.IsNullOrWhiteSpace(sortDirection) ? "ASC" : sortDirection,
                    incluirEliminados ?? false);

                // Devolvemos las filas + metadatos de paginación
                return Results.Ok(new
                {
                    pageNumber = pageNumber ?? 1,
                    pageSize = pageSize ?? 10,
                    total,
                    items
                });
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

                // 409 si ya existe
                var exists = await service.GetByIdAsync(parametro.Identificador);
                if (exists is not null)
                {
                    return Results.Conflict(new { message = $"Ya existe un parámetro con el identificador '{parametro.Identificador}'." });
                }

                var creado = await service.CreateAsync(parametro);
                if (creado is null)
                {
                    return Results.Problem("No se pudo crear el parámetro");
                }

                return Results.Created($"/parametro/{creado.Identificador}", creado);
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

                await service.UpdateAsync(parametro);

                // No dependemos del conteo de filas: el SP usa SET NOCOUNT ON y el
                // trigger INSTEAD OF UPDATE hace que ExecuteAsync devuelva 0 aunque
                // el UPDATE sí se aplicó. Verificamos leyendo el estado actual.
                var current = await service.GetByIdAsync(id) ?? parametro;
                return Results.Ok(current);
            })
            .WithName("UpdateParametro")
            .WithOpenApi();

            // DELETE /parametro/{id} -> Eliminación LÓGICA (pone Estado = 0)
            group.MapDelete("/{id}", async ([FromServices] IParametroService service, string id) =>
            {
                var exists = await service.GetByIdAsync(id);
                if (exists is null)
                {
                    return Results.NotFound();
                }

                // No dependemos del conteo de filas (mismo motivo que en el PUT:
                // SET NOCOUNT ON en el SP hace que ExecuteAsync devuelva 0
                // aunque el soft delete sí se aplicó).
                await service.LogicDeleteAsync(id);
                return Results.NoContent();
            })
            .WithName("DeleteParametro")
            .WithOpenApi();
        }

        // Validaciones según criterios de aceptación de SRV15
        private static List<string> Validar(Parametro parametro)
        {
            var errores = new List<string>();

            if (string.IsNullOrWhiteSpace(parametro.Identificador))
            {
                errores.Add("El identificador del parámetro es requerido y no puede ser vacío ni espacios en blanco.");
            }
            else
            {
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