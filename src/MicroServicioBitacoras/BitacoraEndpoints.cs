using MicroServicioBitacoras.Entities;
using MicroServicioBitacoras.Services;
using Microsoft.AspNetCore.Mvc;

namespace MicroServicioBitacoras
{
    public static class BitacoraEndpoints
    {
        public static void MapBitacoraEndpoints(this IEndpointRouteBuilder routes)
        {
            var group = routes
                .MapGroup("/bitacora")
                .WithTags(nameof(Bitacora));

            // GET /bitacora -> Consultar paginado
            group.MapGet("/", async (
                [FromServices] IBitacoraService service,
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
                    sortColumn ?? "",
                    string.IsNullOrWhiteSpace(sortDirection) ? "ASC" : sortDirection,
                    incluirEliminados ?? false);

                return Results.Ok(new
                {
                    pageNumber = pageNumber ?? 1,
                    pageSize = pageSize ?? 10,
                    total,
                    items
                });
            })
            .WithName("GetAllBitacoras")
            .WithOpenApi();

            // POST /bitacora -> Registrar una bitácora
            group.MapPost("/", async ([FromServices] IBitacoraService service, [FromBody] Bitacora bitacora) =>
            {
                var errores = Validar(bitacora);
                if (errores.Count > 0)
                {
                    return Results.BadRequest(new { errores });
                }

                var creada = await service.CreateAsync(bitacora);
                if (creada is null)
                {
                    return Results.Problem("No se pudo registrar la bitácora");
                }

                return Results.Created($"/bitacora/{creada.BitacoraID}", creada);
            })
            .WithName("CreateBitacora")
            .WithOpenApi();
        }

        // Validaciones según criterios de aceptación de SRV9
        private static List<string> Validar(Bitacora bitacora)
        {
            var errores = new List<string>();

            // Usuario que ejecuta la acción: requerido (GUID no vacío)
            if (bitacora.UsuarioID == Guid.Empty)
            {
                errores.Add("El usuario que ejecuta la acción es requerido.");
            }

            // Descripción: requerida, no vacía ni solo espacios en blanco
            if (string.IsNullOrWhiteSpace(bitacora.Descripcion))
            {
                errores.Add("La descripción de la acción es requerida y no puede ser vacía ni espacios en blanco.");
            }
            else if (bitacora.Descripcion.Length > 500)
            {
                errores.Add("La descripción no puede tener más de 500 caracteres.");
            }

            return errores;
        }
    }
}