using MicroservicioAreas.Entities;

namespace MicroservicioAreas.Repository
{
    public interface IAreaRepository
    {
        Task<IEnumerable<Area>> ObtenerTodas();
        Task<Area> ObtenerPorId(Guid id);
        Task<Area> Crear(Area area);
        Task<Area> Actualizar(Area area);
        Task<bool> Eliminar(Guid id);
    }
}