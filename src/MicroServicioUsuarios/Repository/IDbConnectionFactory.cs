using System.Data;

namespace MicroServicioAuth.Repository
{
    public interface IDbConnectionFactory
    {
        IDbConnection CreateConnection();
    }
}
