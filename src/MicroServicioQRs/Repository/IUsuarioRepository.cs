using MicroServicioQRs.Entities;

namespace MicroServicioQRs.Repository
{
    public interface IUsuarioRepository
    {
        // GRD3: busca por llave primaria para validar el QR escaneado.
        Task<Usuario> ObtenerPorId(Guid id);

        // USR3: busca por email (via EmailXUsuarios → Usuarios).
        Task<Usuario> ObtenerPorEmail(string email);
    }
}