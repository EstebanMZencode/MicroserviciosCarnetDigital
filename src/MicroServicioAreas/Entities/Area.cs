using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MicroservicioAreas.Entities
{
    [Table("AreasTrabajo")]
    public class Area
    {
        [Key]
        [Column("AreaTrabID")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid AreaTrabID { get; set; }

        [Required]
        [StringLength(200)]
        public string NombreAreaTrab { get; set; }

        [Required]
        public Guid InstitucionID { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        public DateTime FechaModificacion { get; set; } = DateTime.UtcNow;

        public bool Estado { get; set; } = true;
    }
}