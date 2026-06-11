using MicroservicioTiposUsuario.Entities;

namespace MicroservicioTiposUsuario.Repository
{
    public interface ITipoUsuarioRepository
    {
        Task<IEnumerable<TipoUsuario>> ObtenerTodos();
        Task<TipoUsuario> ObtenerPorId(Guid id);
        Task<TipoUsuario> Crear(TipoUsuario tipoUsuario);
        Task<TipoUsuario> Actualizar(TipoUsuario tipoUsuario);
        Task<bool> Eliminar(Guid id);
    }
}