namespace MicroServicioInstituciones.Entities;

public class Institucion
{
    public Guid InstitucionID { get; set; }
    public string NombreInstitucion { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public bool Estado { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime FechaModificacion { get; set; }
    public List<string> Dominios { get; set; } = new();
}

public class InstitucionRequest
{
    public string NombreInstitucion { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public List<string> Dominios { get; set; } = new();
}