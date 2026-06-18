using MicroservicioAreas.Entities;
using MicroservicioAreas.Services;

namespace MicroservicioAreas
{
    public static class AreaEndpoints
    {
        public static void MapAreaEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/api/area")
                .WithName("Areas")
                .WithOpenApi();

            group.MapGet("/", ObtenerTodas)
                .WithName("ObtenerTodasAreas")
                .WithOpenApi();

            group.MapGet("/{id:guid}", ObtenerPorId)
                .WithName("ObtenerAreaPorId")
                .WithOpenApi();

            group.MapPost("/", Crear)
                .WithName("CrearArea")
                .WithOpenApi();

            group.MapPut("/{id:guid}", Actualizar)
                .WithName("ActualizarArea")
                .WithOpenApi();

            group.MapDelete("/{id:guid}", Eliminar)
                .WithName("EliminarArea")
                .WithOpenApi();
        }

        private static async Task<IResult> ObtenerTodas(IAreaService service)
        {
            try
            {
                var areas = await service.ObtenerTodas();
                return Results.Ok(areas);
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message, statusCode: 500);
            }
        }

        private static async Task<IResult> ObtenerPorId(Guid id, IAreaService service)
        {
            try
            {
                var area = await service.ObtenerPorId(id);
                return Results.Ok(area);
            }
            catch (Exception ex)
            {
                return Results.NotFound(new { error = ex.Message });
            }
        }

        private static async Task<IResult> Crear(Area area, IAreaService service)
        {
            try
            {
                var resultado = await service.Crear(area);
                return Results.Created($"/api/area/{resultado.AreaTrabID}", resultado);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        }

        private static async Task<IResult> Actualizar(Guid id, Area area, IAreaService service)
        {
            try
            {
                area.AreaTrabID = id;
                var resultado = await service.Actualizar(area);
                return Results.Ok(resultado);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        }

        private static async Task<IResult> Eliminar(Guid id, IAreaService service)
        {
            try
            {
                var areaEliminada = await service.Eliminar(id);
                return Results.Ok(new { mensaje = "Área eliminada correctamente", data = areaEliminada });
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        }
    }
}