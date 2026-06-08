using MicroservicioCarreras.Entities;
using MicroservicioCarreras.Services;

namespace MicroservicioCarreras
{
    public static class CarreraEndpoints
    {
        public static void MapCarreraEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/api/carrera")
                .WithName("Carreras")
                .WithOpenApi();

            group.MapGet("/", ObtenerTodas)
                .WithName("ObtenerTodasCarreras")
                .WithOpenApi();

            group.MapGet("/{id:guid}", ObtenerPorId)
                .WithName("ObtenerCarreraPorId")
                .WithOpenApi();

            group.MapPost("/", Crear)
                .WithName("CrearCarrera")
                .WithOpenApi();

            group.MapPut("/{id:guid}", Actualizar)
                .WithName("ActualizarCarrera")
                .WithOpenApi();

            group.MapDelete("/{id:guid}", Eliminar)
                .WithName("EliminarCarrera")
                .WithOpenApi();
        }

        private static async Task<IResult> ObtenerTodas(ICarreraService service)
        {
            try
            {
                var carreras = await service.ObtenerTodas();
                return Results.Ok(carreras);
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message, statusCode: 500);
            }
        }

        private static async Task<IResult> ObtenerPorId(Guid id, ICarreraService service)
        {
            try
            {
                var carrera = await service.ObtenerPorId(id);
                return Results.Ok(carrera);
            }
            catch (Exception ex)
            {
                return Results.NotFound(new { error = ex.Message });
            }
        }

        private static async Task<IResult> Crear(Carrera carrera, ICarreraService service)
        {
            try
            {
                var resultado = await service.Crear(carrera);
                return Results.Created($"/api/carrera/{resultado.CarreraID}", resultado);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        }

        private static async Task<IResult> Actualizar(Guid id, Carrera carrera, ICarreraService service)
        {
            try
            {
                carrera.CarreraID = id;
                var resultado = await service.Actualizar(carrera);
                return Results.Ok(resultado);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        }

        private static async Task<IResult> Eliminar(Guid id, ICarreraService service)
        {
            try
            {
                var resultado = await service.Eliminar(id);
                return Results.Ok(new { mensaje = "Carrera eliminada correctamente", resultado });
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        }
    }
}