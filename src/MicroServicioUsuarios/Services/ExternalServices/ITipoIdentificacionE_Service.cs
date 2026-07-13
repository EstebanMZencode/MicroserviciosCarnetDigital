using MicroServicioUsuarios.Entities;
using static MicroServicioUsuarios.Services.ExternalServices.TipoIdentificacionE_Service;

namespace MicroServicioUsuarios.Services.ExternalServices
{
    public interface ITipoIdentificacionE_Service
    {
        public Task<(TipoIdentificacionResponse, string[] error)> ValidarTipoIdentificacionAsync(string tipoIdentificacionID, string token);
    }
}
