using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MicroservicioTiposUsuario.Entities
{
    [Table("TiposUsuarios", Schema = "Carnet_Identity_User")]
    public class TipoUsuario
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid TipoUsuarioID { get; set; }

        [Required]
        [StringLength(50)]
        public string NombreTipoUsuario { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime FechaCreacion { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime FechaModificacion { get; set; }

        public bool Estado { get; set; } = true;
    }
}