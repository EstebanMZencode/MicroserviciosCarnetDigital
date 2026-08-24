using MicroServicioQRs.Entities;
using Microsoft.EntityFrameworkCore;

namespace MicroServicioQRs.Repository
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly QrDbContext _context;

        private static readonly Guid EstadoActivo =
            Guid.Parse("6176376E-1E47-44FB-95E5-FB28D966842E");

        public UsuarioRepository(QrDbContext context)
        {
            _context = context;
        }

        public async Task<Usuario> ObtenerPorId(Guid id)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.UsuarioID == id && u.EstadoID == EstadoActivo);

            if (usuario == null) throw new Exception("Usuario no encontrado o inactivo");
            return usuario;
        }

        public async Task<Usuario> ObtenerPorEmail(string email)
        {
            // El email vive en EmailXUsuarios — se hace JOIN con Usuarios
            var usuario = await (
                from exu in _context.EmailXUsuarios
                join u in _context.Usuarios on exu.UsuarioID equals u.UsuarioID
                where exu.Email.ToLower() == email.ToLower()
                   && exu.Estado == true
                   && u.EstadoID == EstadoActivo
                select u
            ).FirstOrDefaultAsync();

            if (usuario == null) throw new Exception("Usuario no encontrado o inactivo");
            return usuario;
        }
    }
}