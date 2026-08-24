using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MicroServicioQRs.Entities
{
    [Table("EmailXUsuarios", Schema = "Carnet_Identity_User")]
    public class EmailXUsuario
    {
        [Key]
        [StringLength(255)]
        public string Email { get; set; } = string.Empty;

        public Guid UsuarioID { get; set; }

        public Guid InstitucionID { get; set; }

        public bool Estado { get; set; }
    }
}