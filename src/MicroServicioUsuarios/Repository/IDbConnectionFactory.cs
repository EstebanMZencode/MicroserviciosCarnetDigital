using System.Data;

namespace MicroServicioUsuarios.Repository
{
    public interface IDbConnectionFactory
    {
        IDbConnection CreateConnection();
    }
}
