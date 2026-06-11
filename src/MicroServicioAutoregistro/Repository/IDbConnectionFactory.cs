using Microsoft.Data.SqlClient;

namespace MicroServicioAutoregistro.Repository;

public interface IDbConnectionFactory
{
    SqlConnection CreateConnection();
}