namespace SitioAdministrativoCarnetDigital.Services.MicroServicioAuth
{
    public interface IAuthApiClient
    {
        // Llama a POST /api/auth/login de MicroServicioAuth.
        // Los tres valores van en headers (no en el body), tal como lo espera el microservicio.
        // tipoUsuario es el GUID del tipo "Administrador" definido en appsettings "TipoUsuario".
        Task<ApiResult<LoginResponseDto>> LoginAsync(
            string usuario,
            string contrasena,
            string tipoUsuario);
    }
}
