namespace MicroServicioInstituciones.Entities;

public class Institucion
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;

    // Los dominios vienen de la tabla DominiosInstituciones (relación 1:N)
    public List<string> Dominios { get; set; } = new();
}

public class DominioInstitucion
{
    public int Id { get; set; }
    public int InstitucionId { get; set; }
    public string Dominio { get; set; } = string.Empty;
}

// DTO para crear/modificar
public class InstitucionRequest
{
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public List<string> Dominios { get; set; } = new();
}