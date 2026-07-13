using MicroServicioUsuarios.Entities;

namespace MicroServicioUsuarios.Repository
{
    public interface IUsuarioRepository
    {
        public Task CrearUsuarioDBAsync(UsuarioRequest usuario);
    }
}
