namespace MicroServicioUsuarios.Entities
{
    // Respuesta JSON del GET /api/usuario/{email}
    public class UsuarioDetalleResponse
    {
        public string       Identificacion { get; set; } = null!;
        public string       NombreCompleto { get; set; } = null!;
        public string       TipoUsuario    { get; set; } = null!;
        // Lista de nombres de carrera — vacía si el usuario tiene áreas en su lugar
        public List<string> Carreras       { get; set; } = new();
        // Lista de nombres de área de trabajo — vacía si el usuario tiene carreras
        public List<string> Areas          { get; set; } = new();
    }

    // Resultado plano que devuelve la consulta SQL antes de resolver nombres externos
    public class UsuarioDetalleDB
    {
        public string    Identificacion    { get; set; } = null!;
        public string    NombreCompleto    { get; set; } = null!;
        public string    NombreTipoUsuario { get; set; } = null!;
        public List<Guid> CarreraIDs       { get; set; } = new();
        public List<Guid> AreaIDs          { get; set; } = new();
    }
}
