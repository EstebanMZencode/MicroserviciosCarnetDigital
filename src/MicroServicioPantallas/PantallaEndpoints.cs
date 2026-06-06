using System.Text.RegularExpressions;
using MicroServicioPantallas.Entities;
using MicroServicioPantallas.Services;
using Microsoft.AspNetCore.Mvc;

namespace MicroServicioPantallas
{
    public static class PantallaEndpoints
    {
        public static void MapPantallaEndpoints(this IEndpointRouteBuilder routes)
        {
            var group = routes
                .MapGroup("/pantallas")
                .WithTags(nameof(Pantalla));

            // GET /pantallas -> Obtener todas
            group.MapGet("/", async ([FromServices] IPantallaService service) =>
            {
                return Results.Ok(await service.GetAllAsync());
            })
            .WithName("GetAllPantallas")
            .WithOpenApi();

            // GET /pantallas/{id} -> Obtener por llave primaria
            group.MapGet("/{id}", async ([FromServices] IPantallaService service, int id) =>
            {
                var p = await service.GetByIdAsync(id);
                return p is null ? Results.NotFound() : Results.Ok(p);
            })
            .WithName("GetPantallaById")
            .WithOpenApi();

            // POST /pantallas -> Crear
            group.MapPost("/", async ([FromServices] IPantallaService service, [FromBody] Pantalla pantalla) =>
            {
                var errores = Validar(pantalla);
                if (errores.Count > 0)
                {
                    return Results.BadRequest(new { errores });
                }

                var nuevoId = await service.CreateAsync(pantalla);
                if (nuevoId <= 0)
                {
                    return Results.Problem("No se pudo crear la pantalla");
                }

                pantalla.PantallaID = nuevoId;
                return Results.Created($"/pantallas/{nuevoId}", pantalla);
            })
            .WithName("CreatePantalla")
            .WithOpenApi();

            // PUT /pantallas/{id} -> Modificar
            group.MapPut("/{id}", async ([FromServices] IPantallaService service, int id, [FromBody] Pantalla pantalla) =>
            {
                var errores = Validar(pantalla);
                if (errores.Count > 0)
                {
                    return Results.BadRequest(new { errores });
                }

                var exists = await service.GetByIdAsync(id);
                if (exists is null)
                {
                    return Results.NotFound();
                }

                pantalla.PantallaID = id;
                var updated = await service.UpdateAsync(pantalla);
                if (updated <= 0)
                {
                    return Results.Problem("No se pudo modificar la pantalla");
                }

                var current = await service.GetByIdAsync(id) ?? pantalla;
                return Results.Ok(current);
            })
            .WithName("UpdatePantalla")
            .WithOpenApi();

            // DELETE /pantallas/{id} -> Eliminar
            group.MapDelete("/{id}", async ([FromServices] IPantallaService service, int id) =>
            {
                var exists = await service.GetByIdAsync(id);
                if (exists is null)
                {
                    return Results.NotFound();
                }

                var deleted = await service.DeleteAsync(id);
                return deleted > 0 ? Results.NoContent() : Results.Problem("No se pudo eliminar la pantalla");
            })
            .WithName("DeletePantalla")
            .WithOpenApi();
        }

        // Validaciones según criterios de aceptación de SRV7
        private static List<string> Validar(Pantalla pantalla)
        {
            var errores = new List<string>();

            // Solo letras (incluye acentos/ñ), números y espacios
            var soloLetrasNumerosEspacios = @"^[a-zA-Z0-9 áéíóúÁÉÍÓÚñÑ]+$";

            if (string.IsNullOrWhiteSpace(pantalla.NombrePantalla))
            {
                errores.Add("El nombre de la pantalla es requerido y no puede ser vacío ni espacios en blanco.");
            }
            else
            {
                if (pantalla.NombrePantalla.Length > 100)
                {
                    errores.Add("El nombre no puede tener más de 100 caracteres.");
                }
                if (!Regex.IsMatch(pantalla.NombrePantalla, soloLetrasNumerosEspacios))
                {
                    errores.Add("El nombre solo permite letras, números y espacios.");
                }
            }

            if (string.IsNullOrWhiteSpace(pantalla.Descripcion))
            {
                errores.Add("La descripción es requerida y no puede ser vacía ni espacios en blanco.");
            }
            else
            {
                if (pantalla.Descripcion.Length > 500)
                {
                    errores.Add("La descripción no puede tener más de 500 caracteres.");
                }
                if (!Regex.IsMatch(pantalla.Descripcion, soloLetrasNumerosEspacios))
                {
                    errores.Add("La descripción solo permite letras, números y espacios.");
                }
            }

            if (string.IsNullOrWhiteSpace(pantalla.Ruta))
            {
                errores.Add("La ruta de acceso es requerida y no puede ser vacía ni espacios en blanco.");
            }
            else if (pantalla.Ruta.Length > 500)
            {
                errores.Add("La ruta no puede tener más de 500 caracteres.");
            }

            return errores;
        }
    }
}
