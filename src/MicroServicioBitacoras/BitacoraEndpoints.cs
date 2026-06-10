using MicroServicioBitacoras.Entities;
using MicroServicioBitacoras.Services;
using MicroServicioBitacoras.Security;
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

            group.MapGet("/", async (
                [FromServices] IBitacoraService service,
                [FromServices] TokenValidator tokenValidator,
                [FromHeader(Name = "token")] string? token,
                int? pageNumber,
                int? pageSize,
                string? searchTerm,
                string? sortColumn,
                string? sortDirection,
                bool? incluirEliminados) =>
            {
                if (!await tokenValidator.EsValidoAsync(token))
                    return Results.Unauthorized();

                var (items, total) = await service.GetPaginadoAsync(
                    pageNumber ?? 1,
                    pageSize ?? 10,
                    searchTerm,
                    sortColumn ?? "",
                    string.IsNullOrWhiteSpace(sortDirection) ? "ASC" : sortDirection,
                    incluirEliminados ?? false);

                return Results.Ok(new { pageNumber = pageNumber ?? 1, pageSize = pageSize ?? 10, total, items });
            })
            .WithName("GetAllBitacoras")
            .WithOpenApi();

            group.MapPost("/", async (
                [FromServices] IBitacoraService service,
                [FromServices] TokenValidator tokenValidator,
                [FromHeader(Name = "token")] string? token,
                [FromBody] Bitacora bitacora) =>
            {
                if (!await tokenValidator.EsValidoAsync(token))
                    return Results.Unauthorized();

                var errores = Validar(bitacora);
                if (errores.Count > 0)
                    return Results.BadRequest(new { errores });

                var creada = await service.CreateAsync(bitacora);
                if (creada is null)
                    return Results.Problem("No se pudo registrar la bitácora");

                return Results.Created($"/bitacora/{creada.BitacoraID}", creada);
            })
            .WithName("CreateBitacora")
            .WithOpenApi();
        }

        private static List<string> Validar(Bitacora bitacora)
        {
            var errores = new List<string>();

            if (bitacora.UsuarioID == Guid.Empty)
                errores.Add("El usuario que ejecuta la acción es requerido.");

            if (string.IsNullOrWhiteSpace(bitacora.Descripcion))
                errores.Add("La descripción de la acción es requerida y no puede ser vacía ni espacios en blanco.");
            else if (bitacora.Descripcion.Length > 500)
                errores.Add("La descripción no puede tener más de 500 caracteres.");

            return errores;
        }
    }
}