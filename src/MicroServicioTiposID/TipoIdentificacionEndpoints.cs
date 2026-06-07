using MicroServicioTiposID.Entities;
using MicroServicioTiposID.Services;

namespace MicroServicioTiposID;

public static class TipoIdentificacionEndpoints
{
    public static void MapTipoIdentificacionEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/tiposidentificacion").RequireAuthorization();

        // GET /tiposidentificacion
        group.MapGet("/", async (ITipoIdentificacionService service) =>
            Results.Ok(await service.GetAllAsync()));

        // GET /tiposidentificacion/{id}
        group.MapGet("/{id:int}", async (int id, ITipoIdentificacionService service) =>
        {
            var tipo = await service.GetByIdAsync(id);
            return tipo is null
                ? Results.NotFound(new { message = "Tipo de identificación no encontrado." })
                : Results.Ok(tipo);
        });

        // POST /tiposidentificacion
        group.MapPost("/", async (TipoIdentificacion tipo, ITipoIdentificacionService service) =>
        {
            var (success, message, newId) = await service.CreateAsync(tipo);
            return success
                ? Results.Created($"/tiposidentificacion/{newId}", new { id = newId, message })
                : Results.BadRequest(new { message });
        });

        // PUT /tiposidentificacion/{id}
        group.MapPut("/{id:int}", async (int id, TipoIdentificacion tipo, ITipoIdentificacionService service) =>
        {
            var (success, message) = await service.UpdateAsync(id, tipo);
            return success
                ? Results.Ok(new { message })
                : Results.BadRequest(new { message });
        });

        // DELETE /tiposidentificacion/{id}
        group.MapDelete("/{id:int}", async (int id, ITipoIdentificacionService service) =>
        {
            var (success, message) = await service.DeleteAsync(id);
            return success
                ? Results.Ok(new { message })
                : Results.NotFound(new { message });
        });
    }
}