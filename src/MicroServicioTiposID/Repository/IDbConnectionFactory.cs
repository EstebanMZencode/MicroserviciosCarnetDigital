using MySqlConnector;

namespace MicroServicioTiposID.Repository;

public interface IDbConnectionFactory
{
    MySqlConnection CreateConnection();
}