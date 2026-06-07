using MicroServicioAutoregistro.Entities;
using MicroServicioAutoregistro.Services;

namespace MicroServicioAutoregistro;

public static class AutoregistroEndpoints
{
    public static void MapAutoregistroEndpoints(this WebApplication app)
    {
        // Sin RequireAuthorization() — SRV11 no requiere JWT
        var group = app.MapGroup("/autoregistro");

        // POST /autoregistro — registrar nuevo usuario
        group.MapPost("/", async (UsuarioRegistro usuario, IAutoregistroService service) =>
        {
            var (success, message) = await service.RegistrarAsync(usuario);
            return success
                ? Results.Created("/autoregistro", new { message })
                : Results.BadRequest(new { message });
        });

        // GET /autoregistro/confirmar?token=... — confirmar cuenta via enlace del email
        group.MapGet("/confirmar", async (string token, IAutoregistroService service) =>
        {
            var (success, message) = await service.ConfirmarAsync(token);
            return success
                ? Results.Ok(new { message })
                : Results.BadRequest(new { message });
        });
    }
}