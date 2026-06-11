using MicroservicioCarreras.Entities;

namespace MicroservicioCarreras.Repository
{
    public interface ICarreraRepository
    {
        Task<IEnumerable<Carrera>> ObtenerTodas();
        Task<Carrera> ObtenerPorId(Guid id);
        Task<Carrera> Crear(Carrera carrera);
        Task<Carrera> Actualizar(Carrera carrera);
        Task<bool> Eliminar(Guid id);
    }
}