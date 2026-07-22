namespace SitioAdministrativoCarnetDigital.Services.MicroServicioCarreras
{
    public class CarreraDto
    {
        public Guid CarreraID { get; set; }
        public string NombreCarrera { get; set; } = string.Empty;
        public string DirectorCarrera { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public Guid InstitucionID { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime FechaModificacion { get; set; }
        public bool Estado { get; set; }
    }
}