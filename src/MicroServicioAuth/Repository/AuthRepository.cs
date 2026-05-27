using Dapper;
using MicroServicioAuth.Entities;

namespace MicroServicioAuth.Repository
{
    public class AuthRepository
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;

        public AuthRepository(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }
    }
}
