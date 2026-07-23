namespace MicroServicioAutoregistro.Entities;

public class UsuarioRegistro
{
    public Guid TipoIdentID { get; set; }
    public string Identificacion { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Contrasena { get; set; } = string.Empty;
    public Guid InstitucionID { get; set; }
    public Guid TipoUsuarioID { get; set; }
    public Guid RolID { get; set; }
    public DateTime FechaVencimientoCarnet { get; set; }
    public List<Guid> CarrerasIDs { get; set; } = new();
    public List<Guid> AreasTrabajoIDs { get; set; } = new();
    public List<string> Telefonos { get; set; } = new();
}