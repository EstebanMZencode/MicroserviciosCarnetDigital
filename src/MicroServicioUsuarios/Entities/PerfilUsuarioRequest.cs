namespace MicroServicioUsuarios.Entities
{
    public class PerfilUsuarioRequest
    {
        public string InstitucionID { get; set; }

        public string TipoUsuarioID { get; set; } // Estudiante, Funcionario, Administrador, etc.

        public string RolID { get; set; } // Rol del usuario dentro de la institución.

        public List<string>? CarrerasID { get; set; } // Lista de carreras asociadas al usuario (Para Estudiantes)

        public List<string>? AreasTrabajoID { get; set; } // Lista de áreas de trabajo asociadas al usuario (Para Funcionario)

        // Datos de inicio de sesión del usuario para este perfil
        public LoginUsuarioRequest LoginData { get; set; }
    }
}
