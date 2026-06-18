using MicroservicioCarreras.Entities;
using MicroServicioCarreras.Repository;
using Microsoft.EntityFrameworkCore;

namespace MicroservicioCarreras.Repository
{
    public class CarreraRepository : ICarreraRepository
    {
        private readonly CarreraDbContext _context;

        public CarreraRepository(CarreraDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Carrera>> ObtenerTodas()
        {
            return await _context.Carreras.Where(c => c.Estado).ToListAsync();
        }

        public async Task<Carrera> ObtenerPorId(Guid id)
        {
            var carrera = await _context.Carreras.FirstOrDefaultAsync(c => c.CarreraID == id);
            if (carrera == null) throw new Exception("Carrera no encontrada");
            return carrera;
        }

        public async Task<Carrera> Crear(Carrera carrera)
        {
            carrera.CarreraID = Guid.NewGuid();
            carrera.Estado = true;

            await _context.Database.ExecuteSqlInterpolatedAsync(
                $@"INSERT INTO [Carnet_Core_User].[Carreras] 
                   ([CarreraID], [NombreCarrera], [DirectorCarrera], [Email], [Telefono], [InstitucionID], [FechaCreacion], [FechaModificacion], [Estado])
                   VALUES ({carrera.CarreraID}, {carrera.NombreCarrera}, {carrera.DirectorCarrera}, {carrera.Email}, {carrera.Telefono}, {carrera.InstitucionID}, {carrera.FechaCreacion}, {carrera.FechaModificacion}, {carrera.Estado})"
            );

            var creada = await _context.Carreras.FirstOrDefaultAsync(c => c.CarreraID == carrera.CarreraID);
            return creada;
        }

        public async Task<Carrera> Actualizar(Carrera carrera)
        {
            var existente = await _context.Carreras.FirstOrDefaultAsync(c => c.CarreraID == carrera.CarreraID);
            if (existente == null) throw new Exception("Carrera no encontrada");

            await _context.Database.ExecuteSqlInterpolatedAsync(
                $@"UPDATE [Carnet_Core_User].[Carreras]
                   SET [NombreCarrera] = {carrera.NombreCarrera},
                       [DirectorCarrera] = {carrera.DirectorCarrera},
                       [Email] = {carrera.Email},
                       [Telefono] = {carrera.Telefono},
                       [InstitucionID] = {carrera.InstitucionID}
                   WHERE [CarreraID] = {carrera.CarreraID}"
            );

            var actualizada = await _context.Carreras.FirstOrDefaultAsync(c => c.CarreraID == carrera.CarreraID);
            return actualizada;
        }

        public async Task<Carrera> Eliminar(Guid id)
        {
            var carrera = await _context.Carreras.FirstOrDefaultAsync(c => c.CarreraID == id);
            if (carrera == null) throw new Exception("Carrera no encontrada");

            await _context.Database.ExecuteSqlInterpolatedAsync(
                $@"UPDATE [Carnet_Core_User].[Carreras]
                   SET [Estado] = 0
                   WHERE [CarreraID] = {id}"
            );

            var eliminada = await _context.Carreras.FirstOrDefaultAsync(c => c.CarreraID == id);
            return eliminada;
        }
    }
}