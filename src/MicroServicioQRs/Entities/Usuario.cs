using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MicroServicioQRs.Entities
{
    // Mapeada a la tabla real Carnet_Identity_User.Usuarios
    [Table("Usuarios")]
    public class Usuario
    {
        [Key]
        public Guid UsuarioID { get; set; }

        public Guid TipoIdentID { get; set; }

        [Required]
        [StringLength(100)]
        public string Identificacion { get; set; }

        [Required]
        [StringLength(255)]
        public string NombreCompleto { get; set; }

        // varchar(max), nullable. Es la foto del carnet.
        public string? FotoBase64 { get; set; }

        public DateTime FechaCreacion { get; set; }

        public DateTime FechaModificacion { get; set; }

        // OJO: EstadoID es un Guid (FK a EstadosUsuarios), NO un bool.
        public Guid EstadoID { get; set; }
    }
}