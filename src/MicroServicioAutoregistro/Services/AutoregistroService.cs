using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using MicroServicioAutoregistro.Entities;
using MicroServicioAutoregistro.Repository;

namespace MicroServicioAutoregistro.Services;

public class AutoregistroService : IAutoregistroService
{
    private readonly AutoregistroRepository _repository;
    private readonly IConfiguration _configuration;

    public AutoregistroService(AutoregistroRepository repository, IConfiguration configuration)
    {
        _repository = repository;
        _configuration = configuration;
    }

    public async Task<(bool Success, string Message)> RegistrarAsync(UsuarioRegistro usuario)
    {
        // Validaciones
        if (string.IsNullOrWhiteSpace(usuario.Email))
            return (false, "El email es requerido.");

        if (!Regex.IsMatch(usuario.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            return (false, "El formato del email no es válido.");

        if (string.IsNullOrWhiteSpace(usuario.NombreCompleto))
            return (false, "El nombre completo es requerido.");

        if (string.IsNullOrWhiteSpace(usuario.Identificacion))
            return (false, "La identificación es requerida.");

        if (string.IsNullOrWhiteSpace(usuario.Contrasena))
            return (false, "La contraseña es requerida.");

        if (usuario.TipoIdentID == Guid.Empty)
            return (false, "El tipo de identificación es requerido.");

        if (usuario.InstitucionID == Guid.Empty)
            return (false, "La institución es requerida.");

        if (usuario.TipoUsuarioID == Guid.Empty)
            return (false, "El tipo de usuario es requerido.");

        if (usuario.RolID == Guid.Empty)
            return (false, "El rol es requerido.");

        if (usuario.FechaVencimientoCarnet == default)
            return (false, "La fecha de vencimiento del carnet es requerida.");

        // Validar dominio del email
        var coreConnection = _configuration.GetConnectionString("CoreConnection")
            ?? throw new InvalidOperationException("CoreConnection no configurado.");

        var dominios = (await _repository.GetDominiosInstitucionAsync(usuario.InstitucionID, coreConnection)).ToList();

        if (!dominios.Any())
            return (false, "Institución no encontrada o sin dominios configurados.");

        var emailDominio = usuario.Email.Split('@').Last().ToLower();
        if (!dominios.Any(d => d.Trim().ToLower() == emailDominio))
            return (false, $"El email debe pertenecer a uno de los dominios: {string.Join(", ", dominios)}");

        // Verificar email duplicado
        if (await _repository.EmailExistsAsync(usuario.Email))
            return (false, "Ya existe un usuario registrado con ese email.");

        // Hash contraseña
        var hash = HashPassword(usuario.Contrasena);

        // Token de confirmación
        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64))
            .Replace("+", "-").Replace("/", "_").Replace("=", "");

        var minutos = int.Parse(_configuration["App:TokenExpirationMinutes"] ?? "15");
        var expiracion = DateTime.UtcNow.AddMinutes(minutos);

        await _repository.CreateUsuarioAsync(usuario, hash, token, expiracion);

        await EnviarEmailConfirmacionAsync(usuario.Email, usuario.NombreCompleto, token);

        return (true, "Registro exitoso. Revisá tu email para confirmar tu cuenta.");
    }

    public async Task<(bool Success, string Message)> ConfirmarAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return (false, "El token es requerido.");

        var (exists, expired, email) = await _repository.GetTokenDataAsync(token);

        if (!exists)
            return (false, "Token no válido o ya fue utilizado.");

        if (expired)
            return (false, "El enlace de confirmación ha vencido. Por favor realizá el registro nuevamente.");

        await _repository.ConfirmarUsuarioAsync(email);
        return (true, "Cuenta confirmada exitosamente. Ya podés iniciar sesión.");
    }

    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }

    private async Task EnviarEmailConfirmacionAsync(string destinatario, string nombre, string token)
    {
        try
        {
            var baseUrl = _configuration["App:BaseUrl"] ?? "http://localhost:5000";
            var minutos = _configuration["App:TokenExpirationMinutes"] ?? "15";
            var enlace = $"{baseUrl}/autoregistro/confirmar?token={token}";

            using var client = new SmtpClient(_configuration["Smtp:Host"], int.Parse(_configuration["Smtp:Port"] ?? "587"))
            {
                Credentials = new NetworkCredential(_configuration["Smtp:User"], _configuration["Smtp:Password"]),
                EnableSsl = true
            };

            var mensaje = new MailMessage(_configuration["Smtp:From"]!, destinatario)
            {
                Subject = "Confirmá tu registro - Carnet Digital CUC",
                Body = $@"<html><body>
                    <h2>¡Hola, {nombre}!</h2>
                    <p>Gracias por registrarte en el sistema de Carnet Digital del CUC.</p>
                    <p>Hacé clic en el siguiente enlace para confirmar tu cuenta:</p>
                    <p><a href=""{enlace}"">Confirmar mi cuenta</a></p>
                    <p><small>Este enlace vence en {minutos} minutos.</small></p>
                    </body></html>",
                IsBodyHtml = true
            };

            await client.SendMailAsync(mensaje);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error enviando email: {ex.Message}");
        }
    }
}