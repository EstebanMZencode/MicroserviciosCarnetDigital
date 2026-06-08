using MicroservicioTiposUsuario.Entities;

namespace MicroservicioTiposUsuario.Services
{
    public interface ITipoUsuarioService
    {
        Task<IEnumerable<TipoUsuario>> ObtenerTodos();
        Task<TipoUsuario> ObtenerPorId(Guid id);
        Task<TipoUsuario> Crear(TipoUsuario tipoUsuario);
        Task<TipoUsuario> Actualizar(TipoUsuario tipoUsuario);
        Task<bool> Eliminar(Guid id);
    }
}