using Microsoft.Data.SqlClient;

namespace MicroServicioInstituciones.Repository;

public interface IDbConnectionFactory
{
    SqlConnection CreateConnection();
}