using MicroServicioAutoregistro.Entities;
using MicroServicioAutoregistro.Services;

namespace MicroServicioAutoregistro;

public static class AutoregistroEndpoints
{
    public static void MapAutoregistroEndpoints(this WebApplication app)
    {
        // Sin RequireAuthorization — SRV11 no requiere JWT según el PDF
        var group = app.MapGroup("/autoregistro");

        // POST /autoregistro
        group.MapPost("/", async (UsuarioRegistro usuario, IAutoregistroService service) =>
        {
            var (success, message) = await service.RegistrarAsync(usuario);
            return success
                ? Results.Created("/autoregistro", new { message })
                : Results.BadRequest(new { message });
        });

        // GET /autoregistro/confirmar?token=...
        group.MapGet("/confirmar", async (string token, IAutoregistroService service) =>
        {
            var (success, message) = await service.ConfirmarAsync(token);
            return success
                ? Results.Ok(new { message })
                : Results.BadRequest(new { message });
        });
    }
}