using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MicroServicioQRs.Entities
{
    // Bitacora segun el documento (pag. 8):
    //   - Fecha de la bitacora
    //   - Usuario que ejecuta la accion
    //   - Descripcion de la accion (Accion realizada + JSON)
    [Table("Bitacoras")]
    public class Bitacora
    {
        [Key]
        public Guid BitacoraID { get; set; }

        public DateTime Fecha { get; set; } = DateTime.UtcNow;

        [StringLength(255)]
        public string Usuario { get; set; }

        public string Descripcion { get; set; }
    }
}