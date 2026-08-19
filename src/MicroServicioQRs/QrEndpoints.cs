using MicroServicioQRs.Dtos;
using MicroServicioQRs.Services;

namespace MicroServicioQRs
{
    public static class QrEndpoints
    {
        public static void MapQrEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/api/qr")
                .WithName("QR")
                .WithOpenApi();

            // USR3: genera y devuelve el QR del usuario autenticado.
            group.MapGet("/{usuarioId:guid}", GenerarQr)
                .WithName("GenerarQr")
                .WithOpenApi();

            // GRD3: consulta el usuario por su llave primaria (para comparar).
            group.MapGet("/usuario/{id:guid}", ConsultarPorLlave)
                .WithName("ConsultarUsuarioPorLlave")
                .WithOpenApi();

            // GRD3: valida dato por dato el JSON escaneado.
            group.MapPost("/validar", Validar)
                .WithName("ValidarQr")
                .WithOpenApi();
        }

        private static async Task<IResult> GenerarQr(Guid usuarioId, IQrService service)
        {
            try
            {
                var resultado = await service.GenerarQr(usuarioId);
                return Results.Ok(resultado);
            }
            catch (Exception ex)
            {
                return Results.NotFound(new { error = ex.Message });
            }
        }

        private static async Task<IResult> ConsultarPorLlave(Guid id, IQrService service)
        {
            try
            {
                var usuario = await service.ConsultarPorLlave(id);
                return Results.Ok(usuario);
            }
            catch (Exception ex)
            {
                return Results.NotFound(new { error = ex.Message });
            }
        }

        private static async Task<IResult> Validar(UsuarioQrDto escaneado, IQrService service)
        {
            try
            {
                var resultado = await service.Validar(escaneado);
                return Results.Ok(resultado);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        }
    }
}