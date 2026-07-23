using Microsoft.Data.SqlClient;

namespace MicroServicioTiposID.Repository;

public interface IDbConnectionFactory
{
    SqlConnection CreateConnection();
}