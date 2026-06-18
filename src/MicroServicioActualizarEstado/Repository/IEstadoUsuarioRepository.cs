namespace MicroServicioActualizarEstadoUsuario.Repository
{
    public interface IEstadoUsuarioRepository
    {
        Task<Entities.EstadoUsuarioResponse?> UpdateEstadoUsuarioAsync(
            string emailUsuario, Guid estadoId);
    }
}