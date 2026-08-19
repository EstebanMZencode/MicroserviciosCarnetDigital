using MicroServicioQRs.Entities;
using Microsoft.EntityFrameworkCore;

namespace MicroServicioQRs.Repository
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly QrDbContext _context;

        // Guid del estado "ACTIVO" en la tabla EstadosUsuarios.
        // Lo sacamos directo de la BD (query que corriste).
        private static readonly Guid EstadoActivo =
            Guid.Parse("6176376E-1E47-44FB-95E5-FB28D966842E");

        public UsuarioRepository(QrDbContext context)
        {
            _context = context;
        }

        public async Task<Usuario> ObtenerPorId(Guid id)
        {
            // Solo usuarios activos, comparando contra el Guid de estado (no un bool).
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.UsuarioID == id && u.EstadoID == EstadoActivo);

            if (usuario == null) throw new Exception("Usuario no encontrado o inactivo");
            return usuario;
        }
    }
}