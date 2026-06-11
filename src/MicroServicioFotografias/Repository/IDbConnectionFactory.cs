using System.Data;

namespace MicroServicioFotografias.Repository
{
    public interface IDbConnectionFactory
    {
        IDbConnection CreateConnection();
    }
}
