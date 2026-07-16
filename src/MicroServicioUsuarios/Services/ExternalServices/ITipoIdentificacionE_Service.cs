using MicroServicioUsuarios.Entities;
using static MicroServicioUsuarios.Services.ExternalServices.TipoIdentificacionE_Service;

namespace MicroServicioUsuarios.Services.ExternalServices
{
    public interface ITipoIdentificacionE_Service
    {
        public Task<(int statusCode, ServicesStatus.ServiceStatus, string message, string[] errors)> ValidarTipoIdentificacionAsync(string tipoIdentificacionID, string token);
    }
}
