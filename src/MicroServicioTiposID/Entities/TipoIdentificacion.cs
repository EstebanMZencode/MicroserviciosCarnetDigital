namespace MicroServicioTiposID.Entities;

public class TipoIdentificacion
{
    public Guid TipoIdentID { get; set; }
    public string NombreTipoIdent { get; set; } = string.Empty;
    public bool Estado { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime FechaModificacion { get; set; }
}

public class TipoIdentificacionRequest
{
    public string NombreTipoIdent { get; set; } = string.Empty;
}