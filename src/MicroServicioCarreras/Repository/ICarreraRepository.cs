using MicroservicioCarreras.Entities;

namespace MicroServicioCarreras.Repository
{
    public interface ICarreraRepository
    {
        Task<IEnumerable<Carrera>> ObtenerTodas();
        Task<Carrera> ObtenerPorId(Guid id);
        Task<Carrera> Crear(Carrera carrera);
        Task<Carrera> Actualizar(Carrera carrera);
        Task<Carrera> Eliminar(Guid id);
    }
}