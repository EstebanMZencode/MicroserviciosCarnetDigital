namespace MicroServicioUsuarios.Services.ExternalServices
{
    public interface IAuthE_Service
    {
        public Task<MicroServicesResponse> ValidarTokenAsync(string token);
    }
}
