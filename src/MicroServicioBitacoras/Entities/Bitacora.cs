using System.ComponentModel.DataAnnotations;

namespace MicroServicioBitacoras.Entities
{
    public class Bitacora
    {
        // PK tipo GUID: la genera la base (NEWSEQUENTIALID), el cliente NO la manda
        public Guid BitacoraID { get; set; }

        // Usuario que ejecuta la acción (GUID, requerido)
        [Required(ErrorMessage = "El usuario que ejecuta la acción es requerido")]
        public Guid UsuarioID { get; set; }

        // Descripción de la acción (requerida, máx 500 según el diagrama)
        [Required(AllowEmptyStrings = false, ErrorMessage = "La descripción de la acción es requerida")]
        [MaxLength(500, ErrorMessage = "La descripción no puede tener más de 500 caracteres")]
        public string Descripcion { get; set; } = null!;

        // Fecha/hora: la pone el servidor (trigger), no el cliente
        public DateTime? FechaHora { get; set; }
    }
}