using System.ComponentModel.DataAnnotations;

namespace MicroServicioParametros.Entities
{
    public class Parametro
    {
        // Identificador del parámetro: texto, máximo 10 caracteres, SOLO letras mayúsculas (SRV15)
        [Required(AllowEmptyStrings = false, ErrorMessage = "El identificador del parámetro es requerido")]
        [MaxLength(10, ErrorMessage = "El identificador no puede tener más de 10 caracteres")]
        [RegularExpression("^[A-Z]+$", ErrorMessage = "El identificador solo permite letras en mayúscula")]
        public string Identificador { get; set; } = null!;

        // Valor del parámetro: texto, formato libre, máximo 500 caracteres (SRV15)
        [Required(AllowEmptyStrings = false, ErrorMessage = "El valor del parámetro es requerido")]
        [MaxLength(500, ErrorMessage = "El valor no puede tener más de 500 caracteres")]
        public string Valor { get; set; } = null!;

        // Campos de auditoría (los maneja el servidor, no el cliente)
        public DateTime? FechaCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public bool Estado { get; set; } = true;
    }
}
