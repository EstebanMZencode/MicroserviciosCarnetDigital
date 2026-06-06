using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MicroservicioAreas.Entities
{
    [Table("AreasTrabajo")]
    public class Area
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AreaTrabajoID { get; set; }

        [Required]
        [StringLength(200)]
        public string NombreAreaTrab { get; set; }

        [Required]
        public int InstitucionID { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        public DateTime FechaModificacion { get; set; } = DateTime.UtcNow;

        public bool Estado { get; set; } = true;
    }
}