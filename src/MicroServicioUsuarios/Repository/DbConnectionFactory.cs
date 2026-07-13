using Microsoft.Data.SqlClient;
using System.Data;

namespace MicroServicioUsuarios.Repository
{
    public class DbConnectionFactory : IDbConnectionFactory
    {
        private readonly IConfiguration _configuration;

        public DbConnectionFactory(IConfiguration configuration)
        {
            this._configuration = configuration;
        }

        public IDbConnection CreateConnection()
        {
            return new SqlConnection(this._configuration.GetConnectionString("DefaultConnection"));
        }
    }
}
