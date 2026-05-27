using MicroServicioAuth.Entities;
using MicroServicioAuth.Repository;

namespace MicroServicioAuth.Services
{
    public class AuthService : IAuthService
    {
        private readonly AuthRepository _authRepository;
        public AuthService(AuthRepository authRepository)
        {
            _authRepository = authRepository;
        }
    }
}
