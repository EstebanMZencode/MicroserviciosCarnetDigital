using MicroServicioUsuarios.Entities;

namespace MicroServicioUsuarios.Services
{
    public interface IUsuarioService
    {
        Task<(UsuarioCreateResponse? data, int statusCode, string? error)> CrearUsuarioAsync(
            UsuarioCreateRequest request, string token);

        // Busca y construye el detalle de un usuario por email.
        // Valida el token contra /api/validate, consulta la BD local y luego
        // resuelve los nombres de carreras y áreas llamando a los microservicios externos.
        Task<(UsuarioDetalleResponse? data, int statusCode, string? error)> GetDetalleByEmailAsync(
            string email, string token);
    }
}