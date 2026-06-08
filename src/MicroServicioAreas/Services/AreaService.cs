using MicroservicioAreas.Entities;
using MicroservicioAreas.Repository;

namespace MicroservicioAreas.Services
{
    public class AreaService : IAreaService
    {
        private readonly IAreaRepository _repository;

        public AreaService(IAreaRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<Area>> ObtenerTodas()
            => await _repository.ObtenerTodas();

        public async Task<Area> ObtenerPorId(Guid id)
            => await _repository.ObtenerPorId(id);

        public async Task<Area> Crear(Area area)
            => await _repository.Crear(area);

        public async Task<Area> Actualizar(Area area)
            => await _repository.Actualizar(area);

        public async Task<bool> Eliminar(Guid id)
            => await _repository.Eliminar(id);
    }
}