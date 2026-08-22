using MicroServicioUsuarios.Entities;

namespace MicroServicioUsuarios.Repository
{
    public interface IUsuarioRepository
    {
        Task<(Guid usuarioID, List<string> emailsCreados)> CrearUsuarioCompletoAsync(
            UsuarioCreateRequest request,
            string passwordHash);

        // Busca los datos locales del usuario por su email institucional.
        // Devuelve null si el email no existe en EmailXUsuarios.
        Task<UsuarioDetalleDB?> GetDetalleByEmailAsync(string email);
    }
}