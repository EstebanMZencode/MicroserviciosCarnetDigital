using System.Data;

namespace MicroServicioPantallas.Repository
{
    public interface IDbConnectionFactory
    {
        IDbConnection CreateConnection();
    }
}