using MicroServicioQRs.Dtos;

namespace MicroServicioQRs.Services
{
    public interface IQrService
    {
        // USR3: genera el QR (imagen PNG) del usuario autenticado, buscando por email.
        Task<QrResponse> GenerarQr(string email);

        // GRD3: consulta el usuario por email (para comparar).
        Task<UsuarioQrDto> ConsultarPorEmail(string email);

        // GRD3: valida dato por dato el JSON escaneado contra la BD.
        Task<ValidacionResponse> Validar(UsuarioQrDto escaneado);
    }
}