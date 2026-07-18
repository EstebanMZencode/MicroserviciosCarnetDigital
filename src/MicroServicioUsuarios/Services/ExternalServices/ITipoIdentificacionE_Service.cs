namespace MicroServicioUsuarios.Services.ExternalServices
{
    public interface ITipoIdentificacionE_Service
    {
        public Task<MicroServicesResponse> ValidarTipoIdentificacionIDAsync(string tipoIdentificacionID, string token);
    }
}
