namespace MicroServicioUsuarios.Services.ExternalServices
{
    public interface IInstitucionE_Service
    {
        public Task<MicroServicesResponse> ValidarInstitucionIDAsync(string institucionID, string token);
    }
}