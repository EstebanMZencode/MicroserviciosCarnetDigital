using Dapper;

namespace MicroServicioAuth.Repository
{
    public class UsuariosRepository
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;

        public UsuariosRepository(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }
    }
}
