using MicroservicioTiposUsuario.Entities;
using MicroServicioTiposUsuario.Repository;
using Microsoft.EntityFrameworkCore;

namespace MicroservicioTiposUsuario.Repository
{
    public class TipoUsuarioRepository : ITipoUsuarioRepository
    {
        private readonly TipoUsuarioDbContext _context;

        public TipoUsuarioRepository(TipoUsuarioDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TipoUsuario>> ObtenerTodos()
        {
            return await _context.TiposUsuarios.Where(t => t.Estado).ToListAsync();
        }

        public async Task<TipoUsuario> ObtenerPorId(Guid id)
        {
            var tipoUsuario = await _context.TiposUsuarios.FirstOrDefaultAsync(t => t.TipoUsuarioID == id);
            if (tipoUsuario == null) throw new Exception("Tipo de usuario no encontrado");
            return tipoUsuario;
        }

        public async Task<TipoUsuario> Crear(TipoUsuario tipoUsuario)
        {
            tipoUsuario.TipoUsuarioID = Guid.NewGuid();
            tipoUsuario.Estado = true;

            await _context.Database.ExecuteSqlInterpolatedAsync(
                $@"INSERT INTO [Carnet_Identity_User].[TiposUsuarios] 
                   ([TipoUsuarioID], [NombreTipoUsuario], [FechaCreacion], [FechaModificacion], [Estado])
                   VALUES ({tipoUsuario.TipoUsuarioID}, {tipoUsuario.NombreTipoUsuario}, {tipoUsuario.FechaCreacion}, {tipoUsuario.FechaModificacion}, {tipoUsuario.Estado})"
            );

            var creado = await _context.TiposUsuarios.FirstOrDefaultAsync(t => t.TipoUsuarioID == tipoUsuario.TipoUsuarioID);
            return creado;
        }

        public async Task<TipoUsuario> Actualizar(TipoUsuario tipoUsuario)
        {
            var existente = await _context.TiposUsuarios.FirstOrDefaultAsync(t => t.TipoUsuarioID == tipoUsuario.TipoUsuarioID);
            if (existente == null) throw new Exception("Tipo de usuario no encontrado");

            await _context.Database.ExecuteSqlInterpolatedAsync(
                $@"UPDATE [Carnet_Identity_User].[TiposUsuarios]
                   SET [NombreTipoUsuario] = {tipoUsuario.NombreTipoUsuario}
                   WHERE [TipoUsuarioID] = {tipoUsuario.TipoUsuarioID}"
            );

            var actualizado = await _context.TiposUsuarios.FirstOrDefaultAsync(t => t.TipoUsuarioID == tipoUsuario.TipoUsuarioID);
            return actualizado;
        }

        public async Task<TipoUsuario> Eliminar(Guid id)
        {
            var tipoUsuario = await _context.TiposUsuarios.FirstOrDefaultAsync(t => t.TipoUsuarioID == id);
            if (tipoUsuario == null) throw new Exception("Tipo de usuario no encontrado");

            await _context.Database.ExecuteSqlInterpolatedAsync(
                $@"UPDATE [Carnet_Identity_User].[TiposUsuarios]
                   SET [Estado] = 0
                   WHERE [TipoUsuarioID] = {id}"
            );

            var eliminado = await _context.TiposUsuarios.FirstOrDefaultAsync(t => t.TipoUsuarioID == id);
            return eliminado;
        }
    }
}