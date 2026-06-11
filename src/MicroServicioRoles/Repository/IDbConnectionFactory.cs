using System.Data;

namespace MicroServicioRoles.Repository
{
    public interface IDbConnectionFactory
    {
        IDbConnection CreateConnection();
    }
}
