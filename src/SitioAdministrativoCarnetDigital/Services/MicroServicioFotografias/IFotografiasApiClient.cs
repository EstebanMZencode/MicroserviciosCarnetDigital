namespace SitioAdministrativoCarnetDigital.Services.MicroServicioFotografias
{
    public interface IFotografiasApiClient
    {
        // GET {FotografiasUrl}/api/usuario/fotografia/{correo}
        // Retorna la foto en Base64 del usuario, o null en fotoBase64 si no tiene.
        Task<ApiResult<FotografiaDto>> ObtenerFotografiaAsync(string token, string correo);

        // PATCH {FotografiasUrl}/api/usuario/fotografia
        // Body JSON: { identificador, fotografia (Base64) }
        Task<ApiResult<bool>> ActualizarFotografiaAsync(string token, string correo, string fotoBase64);

        // DELETE {FotografiasUrl}/api/usuario/fotografia
        // Header: identificador = correo
        Task<ApiResult<bool>> EliminarFotografiaAsync(string token, string correo);
    }
}
