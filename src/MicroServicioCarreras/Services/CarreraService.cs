using MicroservicioCarreras.Entities;
using MicroservicioCarreras.Repository;
using MicroServicioCarreras.Repository;

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
        {
            if (string.IsNullOrWhiteSpace(carrera.NombreCarrera))
                throw new Exception("El nombre de la carrera es requerido");
            if (string.IsNullOrWhiteSpace(carrera.DirectorCarrera))
                throw new Exception("El director de la carrera es requerido");
            if (string.IsNullOrWhiteSpace(carrera.Email))
                throw new Exception("El email es requerido");
            if (!System.Text.RegularExpressions.Regex.IsMatch(carrera.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                throw new Exception("El formato del email no es válido");
            if (string.IsNullOrWhiteSpace(carrera.Telefono))
                throw new Exception("El teléfono es requerido");
            if (!carrera.Telefono.All(char.IsDigit))
                throw new Exception("El teléfono solo permite valores numéricos");
            if (carrera.InstitucionID == Guid.Empty)
                throw new Exception("La institución es requerida");

            return await _repository.Crear(carrera);
        }

        public async Task<Carrera> Actualizar(Carrera carrera)
        {
            if (string.IsNullOrWhiteSpace(carrera.NombreCarrera))
                throw new Exception("El nombre de la carrera es requerido");
            if (string.IsNullOrWhiteSpace(carrera.DirectorCarrera))
                throw new Exception("El director de la carrera es requerido");
            if (string.IsNullOrWhiteSpace(carrera.Email))
                throw new Exception("El email es requerido");
            if (!System.Text.RegularExpressions.Regex.IsMatch(carrera.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                throw new Exception("El formato del email no es válido");
            if (string.IsNullOrWhiteSpace(carrera.Telefono))
                throw new Exception("El teléfono es requerido");
            if (!carrera.Telefono.All(char.IsDigit))
                throw new Exception("El teléfono solo permite valores numéricos");
            if (carrera.InstitucionID == Guid.Empty)
                throw new Exception("La institución es requerida");

            return await _repository.Actualizar(carrera);
        }

        public async Task<Carrera> Eliminar(Guid id)
            => await _repository.Eliminar(id);
    }
}