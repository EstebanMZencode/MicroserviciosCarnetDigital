using MicroservicioAreas.Entities;
using MicroServicioAreas.Repository;
using Microsoft.EntityFrameworkCore;

namespace MicroservicioAreas.Repository
{
    public class AreaRepository : IAreaRepository
    {
        private readonly AreaDbContext _context;

        public AreaRepository(AreaDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Area>> ObtenerTodas()
        {
            return await _context.Areas.Where(a => a.Estado).ToListAsync();
        }

        public async Task<Area> ObtenerPorId(Guid id)
        {
            var area = await _context.Areas.FirstOrDefaultAsync(a => a.AreaTrabID == id);
            if (area == null) throw new Exception("Área no encontrada");
            return area;
        }

        public async Task<Area> Crear(Area area)
        {
            area.AreaTrabID = Guid.NewGuid();
            area.Estado = true;

            await _context.Database.ExecuteSqlInterpolatedAsync(
                $@"INSERT INTO [Carnet_Core_User].[AreasTrabajo] 
                   ([AreaTrabID], [NombreAreaTrab], [InstitucionID], [FechaCreacion], [FechaModificacion], [Estado])
                   VALUES ({area.AreaTrabID}, {area.NombreAreaTrab}, {area.InstitucionID}, {area.FechaCreacion}, {area.FechaModificacion}, {area.Estado})"
            );

            var creada = await _context.Areas.FirstOrDefaultAsync(a => a.AreaTrabID == area.AreaTrabID);
            return creada;
        }

        public async Task<Area> Actualizar(Area area)
        {
            var existente = await _context.Areas.FirstOrDefaultAsync(a => a.AreaTrabID == area.AreaTrabID);
            if (existente == null) throw new Exception("Área no encontrada");

            await _context.Database.ExecuteSqlInterpolatedAsync(
                $@"UPDATE [Carnet_Core_User].[AreasTrabajo]
                   SET [NombreAreaTrab] = {area.NombreAreaTrab},
                       [InstitucionID] = {area.InstitucionID}
                   WHERE [AreaTrabID] = {area.AreaTrabID}"
            );

            var actualizada = await _context.Areas.FirstOrDefaultAsync(a => a.AreaTrabID == area.AreaTrabID);
            return actualizada;
        }

        public async Task<Area> Eliminar(Guid id)
        {
            var area = await _context.Areas.FirstOrDefaultAsync(a => a.AreaTrabID == id);
            if (area == null) throw new Exception("Área no encontrada");

            await _context.Database.ExecuteSqlInterpolatedAsync(
                $@"UPDATE [Carnet_Core_User].[AreasTrabajo]
                   SET [Estado] = 0
                   WHERE [AreaTrabID] = {id}"
            );

            var eliminada = await _context.Areas.FirstOrDefaultAsync(a => a.AreaTrabID == id);
            return eliminada;
        }
    }
}