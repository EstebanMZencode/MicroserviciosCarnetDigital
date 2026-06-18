using MicroServicioActualizarEstadoUsuario.Entities;

namespace MicroServicioActualizarEstadoUsuario.Services
{
    public interface IEstadoUsuarioService
    {
        Task<(EstadoUsuarioResponse? data, int statusCode, string? error)>
            UpdateEstadoUsuarioAsync(string emailUsuario, Guid estadoId, string token);
    }
}