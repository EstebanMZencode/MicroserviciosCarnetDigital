using MicroServicioFotografias.Entities;

namespace MicroServicioFotografias.Services
{
    // Contrato del servicio de fotografías.
    // Cada método valida el token, ejecuta la operación y registra en bitácora.
    public interface IFotografiasService
    {
        // Valida y actualiza la fotografía del usuario en Base64.
        Task<(int statusCode, string? error)> ActualizarFotografiaAsync(
            string email, string fotoBase64, string token);

        // Establece la fotografía del usuario en NULL.
        Task<(int statusCode, string? error)> EliminarFotografiaAsync(
            string email, string token);

        // Recupera la fotografía del usuario en Base64.
        Task<(FotografiaResponse? data, int statusCode, string? error)> ObtenerFotografiaAsync(
            string email, string token);
    }
}
