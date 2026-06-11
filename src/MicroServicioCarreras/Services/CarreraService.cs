using MicroservicioCarreras.Entities;
using MicroservicioCarreras.Repository;

namespace MicroservicioCarreras.Services
{
    public class CarreraService : ICarreraService
    {
        private readonly ICarreraRepository _repository;

        public CarreraService(ICarreraRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<Carrera>> ObtenerTodas()
            => await _repository.ObtenerTodas();

        public async Task<Carrera> ObtenerPorId(Guid id)
            => await _repository.ObtenerPorId(id);

        public async Task<Carrera> Crear(Carrera carrera)
            => await _repository.Crear(carrera);

        public async Task<Carrera> Actualizar(Carrera carrera)
            => await _repository.Actualizar(carrera);

        public async Task<bool> Eliminar(Guid id)
            => await _repository.Eliminar(id);
    }
}