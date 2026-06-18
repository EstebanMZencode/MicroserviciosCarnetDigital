namespace MicroServicioUsuarios.Entities
{
    public class UsuarioFiltroRequest
    {
        public string? Identificacion { get; set; }
        public string? Nombre { get; set; }
        public Guid? TipoUsuarioID { get; set; }
    }
}