using System.Text;
using System.Text.Json;
using MicroServicioFotografias.Entities;
using MicroServicioFotografias.Repository;
using SixLabors.ImageSharp;

namespace MicroServicioFotografias.Services
{
    public class FotografiasService : IFotografiasService
    {
        private readonly FotografiasRepository _repository;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<FotografiasService> _logger;

        private const int MaxTamanioBytes = 1 * 1024 * 1024; // 1 MB

        public FotografiasService(
            FotografiasRepository repository,
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory,
            ILogger<FotografiasService> logger)
        {
            _repository = repository;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        // ── PATCH /usuario/fotografia ─────────────────────────────────────────

        public async Task<(int statusCode, string? error)> ActualizarFotografiaAsync(
            string email, string fotoBase64, string token)
        {
            if (!await ValidarTokenAsync(token))
                return (401, "No autorizado.");

            // Quitar prefijo data URI si viene (data:image/jpeg;base64,...)
            var fotoLimpia = LimpiarPrefixDataUri(fotoBase64);

            // Validar que sea Base64 válido y decodificable como imagen
            byte[] bytes;
            try
            {
                bytes = Convert.FromBase64String(fotoLimpia);
            }
            catch
            {
                return (400, "El formato de la fotografía debe ser Base64.");
            }

            // Validar tamaño máximo 1 MB
            if (bytes.Length > MaxTamanioBytes)
                return (400, "El tamaño de la fotografía no debe ser mayor a 1 MB");

            // Validar que los bytes sean una imagen válida y con proporción 4:3
            try
            {
                using var imagen = Image.Load(bytes);
                if (imagen.Width * 3 != imagen.Height * 4)
                    return (400, "Las dimensiones de la fotografía deben ser en formato 4:3.");
            }
            catch
            {
                return (400, "El formato de la fotografía debe ser Base64.");
            }

            // Buscar usuario en base de datos
            UsuarioFoto? usuario;
            try
            {
                usuario = await _repository.GetUsuarioPorEmailAsync(email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error de conexion a DB en ActualizarFotografiaAsync.");
                return (500, "No fue posible conectarse a la base de datos. Inténtelo más tarde.");
            }

            if (usuario is null)
                return (404, "No se encontró el usuario solicitado");

            var fotoAnterior = usuario.FotoBase64;

            try
            {
                await _repository.ActualizarFotografiaAsync(usuario.UsuarioID, fotoLimpia);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error de conexion a DB al actualizar fotografía.");
                return (500, "No fue posible conectarse a la base de datos. Inténtelo más tarde.");
            }

            _ = RegistrarBitacoraAsync(token, email,
                $"Actualizacion de Fotografia {email}: " +
                $"Foto anterior={ResumirFoto(fotoAnterior)}, " +
                $"Foto nueva={ResumirFoto(fotoLimpia)}");

            return (204, null);
        }

        // ── DELETE /usuario/fotografia ────────────────────────────────────────

        public async Task<(int statusCode, string? error)> EliminarFotografiaAsync(
            string email, string token)
        {
            if (!await ValidarTokenAsync(token))
                return (401, "No autorizado.");

            UsuarioFoto? usuario;
            try
            {
                usuario = await _repository.GetUsuarioPorEmailAsync(email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error de conexion a DB en EliminarFotografiaAsync.");
                return (500, "No fue posible conectarse a la base de datos. Inténtelo más tarde.");
            }

            if (usuario is null)
                return (404, "No se encontró el usuario solicitado");

            var fotoAnterior = usuario.FotoBase64;

            try
            {
                await _repository.EliminarFotografiaAsync(usuario.UsuarioID);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error de conexion a DB al eliminar fotografía.");
                return (500, "No fue posible conectarse a la base de datos. Inténtelo más tarde.");
            }

            _ = RegistrarBitacoraAsync(token, email,
                $"Borrado de Fotografia {email}: " +
                $"Foto anterior={ResumirFoto(fotoAnterior)}");

            return (204, null);
        }

        // ── GET /usuario/fotografia/{id} ──────────────────────────────────────

        public async Task<(FotografiaResponse? data, int statusCode, string? error)> ObtenerFotografiaAsync(
            string email, string token)
        {
            if (!await ValidarTokenAsync(token))
                return (null, 401, "No autorizado.");

            UsuarioFoto? usuario;
            try
            {
                usuario = await _repository.GetUsuarioPorEmailAsync(email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error de conexion a DB en ObtenerFotografiaAsync.");
                return (null, 500, "No se pudo conectarse a la base de datos. Inténtelo más tarde.");
            }

            if (usuario is null)
                return (null, 404, "No se encontró el usuario solicitado.");

            var respuesta = new FotografiaResponse
            {
                Email = email,
                FotoBase64 = usuario.FotoBase64
            };

            _ = RegistrarBitacoraAsync(token, email,
                $"Obtener Fotografia {email}: " +
                $"Foto={ResumirFoto(usuario.FotoBase64)}");

            return (respuesta, 200, null);
        }

        // ── Métodos privados ──────────────────────────────────────────────────

        // Llama al endpoint GET /api/validate del MicroServicioAuth para validar el JWT.
        private async Task<bool> ValidarTokenAsync(string token)
        {
            try
            {
                var validateUrl = _configuration["MicroServicios:AuthValidateUrl"]!;
                var client = _httpClientFactory.CreateClient();

                var request = new HttpRequestMessage(HttpMethod.Get, validateUrl);
                request.Headers.Add("token", token);

                var response = await client.SendAsync(request);
                if (!response.IsSuccessStatusCode) return false;

                var body = await response.Content.ReadAsStringAsync();
                return body.Contains("true", StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudo contactar al servicio de validacion de token.");
                return false;
            }
        }

        // Registra la acción en MicroServicioBitacoras (fire-and-forget).
        private async Task RegistrarBitacoraAsync(string token, string email, string descripcion)
        {
            try
            {
                var bitacoraUrl = _configuration["MicroServicios:BitacoraUrl"]!;
                var client = _httpClientFactory.CreateClient();

                var body = JsonSerializer.Serialize(new { usuarioID = email, descripcion });

                var request = new HttpRequestMessage(HttpMethod.Post, bitacoraUrl)
                {
                    Content = new StringContent(body, Encoding.UTF8, "application/json")
                };

                request.Headers.Add("token", token);

                await client.SendAsync(request);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error al registrar en bitacora para '{Email}'.", email);
            }
        }

        // Quita el prefijo data URI si la foto viene como "data:image/jpeg;base64,XXXX".
        private static string LimpiarPrefixDataUri(string fotoBase64)
        {
            var idx = fotoBase64.IndexOf(",", StringComparison.Ordinal);
            return idx >= 0 ? fotoBase64[(idx + 1)..] : fotoBase64;
        }

        // Retorna los primeros 50 caracteres de la foto para incluir en la descripción de bitácora.
        // La tabla Bitacoras tiene Descripcion VARCHAR(255), por lo que se trunca para no exceder el límite.
        private static string ResumirFoto(string? foto)
        {
            if (string.IsNullOrEmpty(foto)) return "Sin fotografia";
            return foto.Length <= 50 ? foto : foto[..50] + "...";
        }
    }
}
