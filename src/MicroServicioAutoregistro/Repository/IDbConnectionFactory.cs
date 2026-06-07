using MySqlConnector;

namespace MicroServicioAutoregistro.Repository;

public interface IDbConnectionFactory
{
    MySqlConnection CreateConnection();
}