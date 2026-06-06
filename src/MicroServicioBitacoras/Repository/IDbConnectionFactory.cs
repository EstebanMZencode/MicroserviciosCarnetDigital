using System.Data;

namespace MicroServicioBitacoras.Repository
{
    public interface IDbConnectionFactory
    {
        IDbConnection CreateConnection();
    }
}
