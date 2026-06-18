using MicroServicioUsuarios.Entities;

namespace MicroServicioUsuarios.Repository
{
    public interface IUsuarioRepository
    {
        Task<(Guid usuarioID, List<string> emailsCreados)> CrearUsuarioCompletoAsync(
            UsuarioCreateRequest request,
            string passwordHash);
    }
}