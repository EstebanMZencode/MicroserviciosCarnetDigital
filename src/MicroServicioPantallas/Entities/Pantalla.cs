using System.ComponentModel.DataAnnotations;

namespace MicroServicioPantallas.Entities
{
    public class Pantalla
    {
        // PK autoincremental: la genera la base, el cliente NO la manda
        public int PantallaID { get; set; }

        // Nombre de la pantalla (requerido, máx 100, solo letras/números/espacios)
        [Required(AllowEmptyStrings = false, ErrorMessage = "El nombre de la pantalla es requerido")]
        [MaxLength(100, ErrorMessage = "El nombre no puede tener más de 100 caracteres")]
        public string NombrePantalla { get; set; } = null!;

        // Descripción (requerida, máx 500, solo letras/números/espacios)
        [Required(AllowEmptyStrings = false, ErrorMessage = "La descripción es requerida")]
        [MaxLength(500, ErrorMessage = "La descripción no puede tener más de 500 caracteres")]
        public string Descripcion { get; set; } = null!;

        // Ruta de acceso (requerida, máx 500)
        [Required(AllowEmptyStrings = false, ErrorMessage = "La ruta de acceso es requerida")]
        [MaxLength(500, ErrorMessage = "La ruta no puede tener más de 500 caracteres")]
        public string Ruta { get; set; } = null!;

        // Campos de auditoría (los maneja el servidor)
        public DateTime? FechaCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public bool Estado { get; set; } = true;
    }
}
