using MicroservicioTiposUsuario.Entities;
using MicroservicioTiposUsuario.Services;

namespace MicroservicioTiposUsuario
{
    public static class TipoUsuarioEndpoints
    {
        public static void MapTipoUsuarioEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/api/tipousuario")
                .WithName("TiposUsuarios")
                .WithOpenApi();

            group.MapGet("/", ObtenerTodos)
                .WithName("ObtenerTodosTiposUsuarios")
                .WithOpenApi();

            group.MapGet("/{id:guid}", ObtenerPorId)
                .WithName("ObtenerTipoUsuarioPorId")
                .WithOpenApi();

            group.MapPost("/", Crear)
                .WithName("CrearTipoUsuario")
                .WithOpenApi();

            group.MapPut("/{id:guid}", Actualizar)
                .WithName("ActualizarTipoUsuario")
                .WithOpenApi();

            group.MapDelete("/{id:guid}", Eliminar)
                .WithName("EliminarTipoUsuario")
                .WithOpenApi();
        }

        private static async Task<IResult> ObtenerTodos(ITipoUsuarioService service)
        {
            try
            {
                var tiposUsuarios = await service.ObtenerTodos();
                return Results.Ok(tiposUsuarios);
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message, statusCode: 500);
            }
        }

        private static async Task<IResult> ObtenerPorId(Guid id, ITipoUsuarioService service)
        {
            try
            {
                var tipoUsuario = await service.ObtenerPorId(id);
                return Results.Ok(tipoUsuario);
            }
            catch (Exception ex)
            {
                return Results.NotFound(new { error = ex.Message });
            }
        }

        private static async Task<IResult> Crear(TipoUsuario tipoUsuario, ITipoUsuarioService service)
        {
            try
            {
                var resultado = await service.Crear(tipoUsuario);
                return Results.Created($"/api/tipousuario/{resultado.TipoUsuarioID}", resultado);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        }

        private static async Task<IResult> Actualizar(Guid id, TipoUsuario tipoUsuario, ITipoUsuarioService service)
        {
            try
            {
                tipoUsuario.TipoUsuarioID = id;
                var resultado = await service.Actualizar(tipoUsuario);
                return Results.Ok(resultado);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        }

        private static async Task<IResult> Eliminar(Guid id, ITipoUsuarioService service)
        {
            try
            {
                var resultado = await service.Eliminar(id);
                return Results.Ok(new { mensaje = "Tipo de usuario eliminado correctamente", resultado });
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        }
    }
}