namespace SitioAdministrativoCarnetDigital.Services.MicroServicioInstituciones
{
    public class InstitucionDto
    {
        public Guid InstitucionID { get; set; }
        public string NombreInstitucion { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public List<string> Dominios { get; set; } = new();
        public DateTime FechaCreacion { get; set; }
        public DateTime FechaModificacion { get; set; }
        public bool Estado { get; set; }
    }
}