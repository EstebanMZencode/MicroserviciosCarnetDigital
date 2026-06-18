using System.Data;

namespace MicroServicioActualizarEstadoUsuario.Repository
{
    public interface IDbConnectionFactory
    {
        IDbConnection CreateConnection();
    }
}
