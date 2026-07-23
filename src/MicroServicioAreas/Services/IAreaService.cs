using MicroservicioAreas.Entities;

namespace MicroservicioAreas.Services
{
    public interface IAreaService
    {
        Task<IEnumerable<Area>> ObtenerTodas();
        Task<Area> ObtenerPorId(Guid id);
        Task<Area> Crear(Area area);
        Task<Area> Actualizar(Area area);
        Task<Area> Eliminar(Guid id);
    }
}