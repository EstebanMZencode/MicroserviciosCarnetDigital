using System.Data;

namespace MicroServicioParametros.Repository
{
    public interface IDbConnectionFactory
    {
        IDbConnection CreateConnection();
    }
}
