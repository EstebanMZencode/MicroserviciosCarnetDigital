namespace MicroServicioUsuarios.Entities
{
    public class UsuarioCreateRequest
    {
        public Guid TipoIdentID { get; set; }
        public string Identificacion { get; set; } = null!;
        public string NombreCompleto { get; set; } = null!;
        public Guid EstadoID { get; set; }
        public string Password { get; set; } = null!;
        public List<EmailRequest> Emails { get; set; } = new();
        public List<string> Telefonos { get; set; } = new();
        public List<InstitucionRequest> Instituciones { get; set; } = new();
    }

    public class EmailRequest
    {
        public string Email { get; set; } = null!;
        public Guid InstitucionID { get; set; }
    }

    public class InstitucionRequest
    {
        public Guid InstitucionID { get; set; }
        public Guid TipoUsuarioID { get; set; }
        public Guid RolID { get; set; }
        public DateTime FechaVencimientoCarnet { get; set; }
        public List<Guid> Carreras { get; set; } = new();
        public List<Guid> Areas { get; set; } = new();
    }
}