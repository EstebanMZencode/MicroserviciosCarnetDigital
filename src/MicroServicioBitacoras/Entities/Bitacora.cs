using System.ComponentModel.DataAnnotations;

namespace MicroServicioBitacoras.Entities
{
    public class Bitacora
    {
        // PK autoincremental: la genera la base, el cliente NO la manda
        public int BitacoraID { get; set; }

        // Usuario que ejecuta la acción (requerido)
        [Required(ErrorMessage = "El usuario que ejecuta la acción es requerido")]
        public int UsuarioID { get; set; }

        // Descripción de la acción (requerida, máx 255 según el diagrama)
        [Required(AllowEmptyStrings = false, ErrorMessage = "La descripción de la acción es requerida")]
        [MaxLength(255, ErrorMessage = "La descripción no puede tener más de 255 caracteres")]
        public string Descripcion { get; set; } = null!;

        // Fecha/hora del registro: la pone el servidor, no el cliente
        public DateTime? FechaHora { get; set; }
    }
}
