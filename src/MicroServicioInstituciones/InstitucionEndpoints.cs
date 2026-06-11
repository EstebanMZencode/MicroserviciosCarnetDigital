using MicroServicioInstituciones.Entities;
using MicroServicioInstituciones.Services;

namespace MicroServicioInstituciones;

public static class InstitucionEndpoints
{
    public static void MapInstitucionEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/institucion");

        // GET /institucion
        group.MapGet("/", async (IInstitucionService service) =>
            Results.Ok(await service.GetAllAsync()));

        // GET /institucion/{id}
        group.MapGet("/{id:guid}", async (Guid id, IInstitucionService service) =>
        {
            var institucion = await service.GetByIdAsync(id);
            return institucion is null
                ? Results.NotFound(new { message = "Institución no encontrada." })
                : Results.Ok(institucion);
        });

        // POST /institucion
        group.MapPost("/", async (InstitucionRequest request, IInstitucionService service) =>
        {
            var (success, message, newId) = await service.CreateAsync(request);
            return success
                ? Results.Created($"/institucion/{newId}", new { id = newId, message })
                : Results.BadRequest(new { message });
        });

        // PUT /institucion/{id}
        group.MapPut("/{id:guid}", async (Guid id, InstitucionRequest request, IInstitucionService service) =>
        {
            var (success, message) = await service.UpdateAsync(id, request);
            return success
                ? Results.Ok(new { message })
                : Results.BadRequest(new { message });
        });

        // DELETE /institucion/{id}
        group.MapDelete("/{id:guid}", async (Guid id, IInstitucionService service) =>
        {
            var (success, message) = await service.DeleteAsync(id);
            return success
                ? Results.Ok(new { message })
                : Results.NotFound(new { message });
        });
    }
}