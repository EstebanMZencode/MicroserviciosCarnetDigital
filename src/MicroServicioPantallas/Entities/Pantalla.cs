using System.ComponentModel.DataAnnotations;

namespace MicroServicioPantallas.Entities
{
    public class Pantalla
    {
        public Guid PantallaID { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "El nombre de la pantalla es requerido")]
        [MaxLength(100, ErrorMessage = "El nombre no puede tener más de 100 caracteres")]
        public string NombrePantalla { get; set; } = null!;

        [Required(AllowEmptyStrings = false, ErrorMessage = "La descripción es requerida")]
        [MaxLength(500, ErrorMessage = "La descripción no puede tener más de 500 caracteres")]
        public string Descripcion { get; set; } = null!;

        [Required(AllowEmptyStrings = false, ErrorMessage = "La ruta de acceso es requerida")]
        [MaxLength(500, ErrorMessage = "La ruta no puede tener más de 500 caracteres")]
        public string Ruta { get; set; } = null!;

        public DateTime? FechaCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public bool Estado { get; set; } = true;
    }
}
