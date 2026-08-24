using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MicroServicioQRs.Entities
{
    [Table("Usuarios")]
    public class Usuario
    {
        [Key]
        public Guid UsuarioID { get; set; }

        public Guid TipoIdentID { get; set; }

        [Required]
        [StringLength(100)]
        public string Identificacion { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string NombreCompleto { get; set; } = string.Empty;

        public string? FotoBase64 { get; set; }

        public DateTime FechaCreacion { get; set; }

        public DateTime FechaModificacion { get; set; }

        public Guid EstadoID { get; set; }
    }
}