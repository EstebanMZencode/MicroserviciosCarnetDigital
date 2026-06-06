using MySqlConnector;

namespace MicroServicioInstituciones.Repository;

public interface IDbConnectionFactory
{
    MySqlConnection CreateConnection();
}