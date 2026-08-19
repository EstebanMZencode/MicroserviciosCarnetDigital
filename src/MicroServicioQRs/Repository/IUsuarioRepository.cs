using MicroServicioQRs.Entities;

namespace MicroServicioQRs.Repository
{
    public interface IUsuarioRepository
    {
        // Consulta del usuario por su llave primaria.
        // La usan tanto USR3 (para generar el QR) como GRD3 (para validar).
        Task<Usuario> ObtenerPorId(Guid id);
    }
}