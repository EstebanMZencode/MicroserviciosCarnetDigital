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

            // GET /bitacora -> Consultar todas las bitácoras
            group.MapGet("/", async ([FromServices] IBitacoraService service) =>
            {
                return Results.Ok(await service.GetAllAsync());
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

                var nuevoId = await service.CreateAsync(bitacora);
                if (nuevoId <= 0)
                {
                    return Results.Problem("No se pudo registrar la bitácora");
                }

                // Devolvemos el id generado por la base
                bitacora.BitacoraID = nuevoId;
                return Results.Created($"/bitacora/{nuevoId}", bitacora);
            })
            .WithName("CreateBitacora")
            .WithOpenApi();
        }

        // Validaciones según criterios de aceptación de SRV9
        private static List<string> Validar(Bitacora bitacora)
        {
            var errores = new List<string>();

            // Usuario que ejecuta la acción: requerido (debe ser un id válido > 0)
            if (bitacora.UsuarioID <= 0)
            {
                errores.Add("El usuario que ejecuta la acción es requerido y debe ser válido.");
            }

            // Descripción: requerida, no vacía ni solo espacios en blanco
            if (string.IsNullOrWhiteSpace(bitacora.Descripcion))
            {
                errores.Add("La descripción de la acción es requerida y no puede ser vacía ni espacios en blanco.");
            }
            else if (bitacora.Descripcion.Length > 255)
            {
                errores.Add("La descripción no puede tener más de 255 caracteres.");
            }

            return errores;
        }
    }
}
