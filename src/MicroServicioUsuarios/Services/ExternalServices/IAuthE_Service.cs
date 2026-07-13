namespace MicroServicioUsuarios.Services.ExternalServices
{
    public interface IAuthE_Service
    {
        public Task<bool> ValidarTokenAsync(string token);
    }
}
