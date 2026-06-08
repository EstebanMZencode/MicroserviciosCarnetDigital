using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MicroservicioTiposUsuario.Entities
{
    [Table("TiposUsuarios")]
    public class TipoUsuario
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid TipoUsuarioID { get; set; }

        [Required]
        [StringLength(50)]
        public string NombreTipoUsuario { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        public DateTime FechaModificacion { get; set; } = DateTime.UtcNow;

        public bool Estado { get; set; } = true;
    }
}