namespace MicroServicioAuth.Entities
{
    /// <summary>
    /// Resultado del query de validación de credenciales en base de datos.
    /// Combina columnas de Login, EmailXUsuarios, Usuarios y UsuariosXInstituciones
    /// del schema <c>[Carnet_Identity_User]</c>.
    /// </summary>
    public class UserLoginData
    {
        /// <summary>PK del usuario en la tabla Usuarios.</summary>
        public Guid UsuarioID { get; set; }

        /// <summary>Email del usuario. PK en Login y EmailXUsuarios.</summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>Hash BCrypt de la contraseña almacenado en tabla Login.</summary>
        public string PasswordHash { get; set; } = string.Empty;

        /// <summary>FK a TiposUsuarios en la tabla UsuariosXInstituciones.</summary>
        public Guid TipoUsuarioID { get; set; }

        /// <summary>Referencia al rol del usuario dentro de la institución (UsuariosXInstituciones).</summary>
        public Guid RolID { get; set; }
    }
}
