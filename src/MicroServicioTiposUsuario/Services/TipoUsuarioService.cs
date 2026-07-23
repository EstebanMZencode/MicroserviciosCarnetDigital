using MicroservicioTiposUsuario.Entities;
using MicroservicioTiposUsuario.Repository;

namespace MicroservicioTiposUsuario.Services
{
    public class TipoUsuarioService : ITipoUsuarioService
    {
        private readonly ITipoUsuarioRepository _repository;

        public TipoUsuarioService(ITipoUsuarioRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<TipoUsuario>> ObtenerTodos()
            => await _repository.ObtenerTodos();

        public async Task<TipoUsuario> ObtenerPorId(Guid id)
            => await _repository.ObtenerPorId(id);

        public async Task<TipoUsuario> Crear(TipoUsuario tipoUsuario)
        {
            if (string.IsNullOrWhiteSpace(tipoUsuario.NombreTipoUsuario))
                throw new Exception("El nombre del tipo de usuario es requerido");

            return await _repository.Crear(tipoUsuario);
        }

        public async Task<TipoUsuario> Actualizar(TipoUsuario tipoUsuario)
        {
            if (string.IsNullOrWhiteSpace(tipoUsuario.NombreTipoUsuario))
                throw new Exception("El nombre del tipo de usuario es requerido");

            return await _repository.Actualizar(tipoUsuario);
        }

        public async Task<TipoUsuario> Eliminar(Guid id)
            => await _repository.Eliminar(id);
    }
}