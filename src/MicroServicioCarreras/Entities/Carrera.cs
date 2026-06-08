using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MicroservicioCarreras.Entities
{
    [Table("Carreras")]
    public class Carrera
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid CarreraID { get; set; }

        [Required]
        [StringLength(200)]
        public string NombreCarrera { get; set; }

        [Required]
        [StringLength(255)]
        public string DirectorCarrera { get; set; }

        [Required]
        [StringLength(255)]
        public string Email { get; set; }

        [StringLength(255)]
        public string Telefono { get; set; }

        [Required]
        public Guid InstitucionID { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        public DateTime FechaModificacion { get; set; } = DateTime.UtcNow;

        public bool Estado { get; set; } = true;
    }
}