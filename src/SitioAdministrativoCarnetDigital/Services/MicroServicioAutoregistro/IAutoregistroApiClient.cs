namespace SitioAdministrativoCarnetDigital.Services.MicroServicioAutoregistro
{
    public interface IAutoregistroApiClient
    {
        // POST {AutoregistroUrl}/autoregistro
        // No requiere token; cualquier usuario puede registrarse.
        Task<ApiResult<AutoregistroResponseDto>> RegistrarAsync(UsuarioRegistroRequest request);

        // GET {AutoregistroUrl}/autoregistro/confirmar?token={token}
        // Lo invoca el enlace del correo de confirmación.
        Task<ApiResult<AutoregistroResponseDto>> ConfirmarAsync(string token);
    }
}