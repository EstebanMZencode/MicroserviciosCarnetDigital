namespace SitioAdministrativoCarnetDigital.Services.MicroServicioAreasTrabajo
{
    public class AreaTrabajoDto
    {
        public Guid AreaTrabID { get; set; }
        public string NombreAreaTrab { get; set; } = string.Empty;
        public Guid InstitucionID { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime FechaModificacion { get; set; }
        public bool Estado { get; set; }
    }
}