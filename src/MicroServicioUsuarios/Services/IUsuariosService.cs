using MicroServicioUsuarios.Entities;

namespace MicroServicioUsuarios.Services
{
    public interface IUsuarioService
    {
        Task<(UsuarioCreateResponse? data, int statusCode, string? error)> CrearUsuarioAsync(
            UsuarioCreateRequest request, string token);
    }
}