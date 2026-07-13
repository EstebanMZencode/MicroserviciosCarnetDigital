using MicroServicioUsuarios.Entities;
using MicroServicioUsuarios.Services;

namespace MicroServicioUsuarios.Repository
{
    public class UsuarioRepository : IUsuarioRepository 
    {
        private readonly IConfiguration _config;
        private readonly IHttpClientFactory _httpClientFactory;

        public UsuarioRepository(
            IConfiguration config,
            IHttpClientFactory httpClientFactory)
        {
            _config = config;
            _httpClientFactory = httpClientFactory;
        }

        public async Task CrearUsuarioDBAsync(UsuarioRequest usuario)
        {
            
        }
    }
}
