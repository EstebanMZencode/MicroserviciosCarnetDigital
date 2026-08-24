using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MicroServicioQRs.Entities
{
    [Table("EmailXUsuarios", Schema = "Carnet_Identity_User")]
    public class EmailXUsuario
    {
        [Key]
        [StringLength(255)]
        public string Email { get; set; }

        public Guid UsuarioID { get; set; }

        public Guid InstitucionID { get; set; }

        // 1 = activo
        public bool Estado { get; set; }
    }
}