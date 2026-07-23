namespace SitioAdministrativoCarnetDigital.Services.MicroServicioEstadosUsuario
{
    public interface IEstadosUsuarioApiClient
    {
        void SetToken(string token);
        Task CambiarEstadoAsync(string emailUsuario, Guid estadoId);
    }
}