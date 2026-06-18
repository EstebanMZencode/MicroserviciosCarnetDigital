using MicroservicioCarreras.Entities;

namespace MicroservicioCarreras.Services
{
    public interface ICarreraService
    {
        Task<IEnumerable<Carrera>> ObtenerTodas();
        Task<Carrera> ObtenerPorId(Guid id);
        Task<Carrera> Crear(Carrera carrera);
        Task<Carrera> Actualizar(Carrera carrera);
        Task<Carrera> Eliminar(Guid id);
    }
}