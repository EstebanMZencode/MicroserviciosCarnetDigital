namespace MicroServicioUsuarios.Entities
{
    public class UsuarioRequest
    {
        public string TipoIdentificacion { get; set; } // guid del tipo de identificación del usuario
        public string Identificacion { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public List<string>? Telefonos { get; set; } // Lista de números de teléfono del usuario opcionales

        // Un usuario puede tener varios perfiles asociados, por lo que se utiliza una lista de objetos PerfilUsuarioRequest 
        public List<PerfilUsuarioRequest>? Perfiles { get; set; } 
    }
}