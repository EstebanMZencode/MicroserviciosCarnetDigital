namespace MicroServicioAutoregistro.Entities;

public class UsuarioRegistro
{
    public string Email { get; set; } = string.Empty;
    public int TipoIdentificacionId { get; set; }
    public string Identificacion { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public int InstitucionId { get; set; }
    public string Contrasena { get; set; } = string.Empty;
    public int TipoUsuarioId { get; set; }
    public List<int> CarrerasIds { get; set; } = new();
    public List<int> AreasIds { get; set; } = new();
    public List<string> Telefonos { get; set; } = new();
    public int RolId { get; set; }
}