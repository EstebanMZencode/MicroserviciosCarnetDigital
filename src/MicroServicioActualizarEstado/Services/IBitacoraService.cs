namespace MicroServicioActualizarEstadoUsuario.Services
{
    public interface IBitacoraService
    {
        Task RegistrarAsync(string usuario, string accion, string? detalle = null);
    }
}