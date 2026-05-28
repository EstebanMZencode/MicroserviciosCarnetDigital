using MicroServicioAuth.Entities;
using MicroServicioAuth.Repository;

namespace MicroServicioAuth.Services
{
    public class UsuariosService : IUsuariosService
    {
        private readonly AuthRepository _authRepository;
        public UsuariosService(AuthRepository authRepository)
        {
            _authRepository = authRepository;
        }
    }
}
