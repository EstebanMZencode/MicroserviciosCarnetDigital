using System.Text.Json;
using MicroServicioQRs.Dtos;
using MicroServicioQRs.Repository;
using QRCoder;

namespace MicroServicioQRs.Services
{
    public class QrService : IQrService
    {
        private readonly IUsuarioRepository _usuarioRepository;

        public QrService(IUsuarioRepository usuarioRepository)
        {
            _usuarioRepository = usuarioRepository;
        }

        // USR3: genera el QR (imagen PNG en base64) del usuario autenticado.
        public async Task<QrResponse> GenerarQr(Guid usuarioId)
        {
            // 1. Traer el usuario real de la BD (solo activos).
            var usuario = await _usuarioRepository.ObtenerPorId(usuarioId);

            // 2. Armar el DTO que ira DENTRO del QR (solo lo identificador).
            var dto = new UsuarioQrDto
            {
                UsuarioID = usuario.UsuarioID,
                Identificacion = usuario.Identificacion,
                NombreCompleto = usuario.NombreCompleto
            };

            // 3. Serializar a JSON. Este JSON es lo que el guarda leera al escanear.
            string contenidoJson = JsonSerializer.Serialize(dto);

            // 4. Generar la imagen del QR con QRCoder (PngByteQRCode = sin System.Drawing,
            //    funciona en Linux/Plesk sin problemas).
            using var generator = new QRCodeGenerator();
            using var qrData = generator.CreateQrCode(contenidoJson, QRCodeGenerator.ECCLevel.Q);
            var pngQr = new PngByteQRCode(qrData);
            byte[] qrBytes = pngQr.GetGraphic(20); // 20 px por modulo
            string qrBase64 = Convert.ToBase64String(qrBytes);

            // 5. Devolver todo a USR3.
            return new QrResponse
            {
                QrBase64 = qrBase64,
                Contenido = contenidoJson,
                UsuarioID = usuario.UsuarioID
            };
        }

        // GRD3: consulta el usuario por su llave primaria (para que el guarda compare).
        public async Task<UsuarioQrDto> ConsultarPorLlave(Guid usuarioId)
        {
            var usuario = await _usuarioRepository.ObtenerPorId(usuarioId);

            return new UsuarioQrDto
            {
                UsuarioID = usuario.UsuarioID,
                Identificacion = usuario.Identificacion,
                NombreCompleto = usuario.NombreCompleto
            };
        }

        // GRD3: valida dato por dato el JSON escaneado contra la BD.
        public async Task<ValidacionResponse> Validar(UsuarioQrDto escaneado)
        {
            var respuesta = new ValidacionResponse();

            if (escaneado == null || escaneado.UsuarioID == Guid.Empty)
            {
                respuesta.Valido = false;
                respuesta.Mensaje = "El contenido del QR es invalido o esta vacio.";
                return respuesta;
            }

            // Consultar el usuario real por su llave primaria.
            UsuarioQrDto real;
            try
            {
                real = await ConsultarPorLlave(escaneado.UsuarioID);
            }
            catch
            {
                respuesta.Valido = false;
                respuesta.Mensaje = "El usuario del QR no existe o esta inactivo.";
                return respuesta;
            }

            // Comparar dato por dato (como pide la descripcion tecnica de GRD3).
            if (escaneado.Identificacion != real.Identificacion)
                respuesta.Diferencias.Add("La identificacion no coincide.");

            if (escaneado.NombreCompleto != real.NombreCompleto)
                respuesta.Diferencias.Add("El nombre no coincide.");

            // Si no hubo diferencias, el QR es valido.
            respuesta.Valido = respuesta.Diferencias.Count == 0;
            respuesta.Mensaje = respuesta.Valido
                ? "QR valido. Los datos coinciden."
                : "QR invalido. Los datos no coinciden.";

            return respuesta;
        }
    }
}