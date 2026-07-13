using MicroServicioUsuarios.Entities;

namespace MicroServicioUsuarios.Services
{
    public interface IUsuarioService
    {
        public Task<IResult> CreateUsuarioServiceAsync(UsuarioRequest usuario, string authorization);
    }
}
