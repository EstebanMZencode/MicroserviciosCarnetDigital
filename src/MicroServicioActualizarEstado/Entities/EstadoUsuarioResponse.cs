namespace MicroServicioActualizarEstadoUsuario.Entities
{
    public class EstadoUsuarioResponse
    {
        public string Email { get; set; } = null!;
        public Guid UsuarioID { get; set; }
        public string NombreEstado { get; set; } = null!;
        public Guid EstadoID { get; set; }
    }
}