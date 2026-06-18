namespace MicroServicioUsuarios.Entities
{
    public class UsuarioCreateResponse
    {
        public Guid UsuarioID { get; set; }
        public string TipoIdentificacion { get; set; } = null!;
        public string Identificacion { get; set; } = null!;
        public string NombreCompleto { get; set; } = null!;
        public string NombreEstado { get; set; } = null!;
        public List<EmailResponse> Emails { get; set; } = new();
        public List<string> Telefonos { get; set; } = new();
        public List<InstitucionResponse> Instituciones { get; set; } = new();
    }

    public class EmailResponse
    {
        public string Email { get; set; } = null!;
        public string Institucion { get; set; } = null!;
    }

    public class InstitucionResponse
    {
        public string Institucion { get; set; } = null!;
        public string TipoUsuario { get; set; } = null!;
        public string Rol { get; set; } = null!;
        public DateTime FechaVencimientoCarnet { get; set; }
        public List<string> Carreras { get; set; } = new();
        public List<string> Areas { get; set; } = new();
    }
}