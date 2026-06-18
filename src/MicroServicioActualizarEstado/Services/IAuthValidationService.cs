namespace MicroServicioActualizarEstadoUsuario.Services
{
    public interface IAuthValidationService
    {
        Task<bool> ValidarTokenAsync(string token);
    }
}