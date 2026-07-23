using MicroServicioAutoregistro.Entities;

namespace MicroServicioAutoregistro.Services;

public interface IAutoregistroService
{
    Task<(bool Success, string Message)> RegistrarAsync(UsuarioRegistro usuario);
    Task<(bool Success, string Message)> ConfirmarAsync(string token);
}