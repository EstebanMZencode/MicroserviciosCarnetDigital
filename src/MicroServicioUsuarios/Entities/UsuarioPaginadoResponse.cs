namespace MicroServicioUsuarios.Entities
{
    public class UsuarioPaginadoResponse
    {
        public int TotalRegistros { get; set; }
        public int Pagina { get; set; }
        public int Tamano { get; set; }
        public List<UsuarioResumenResponse> Usuarios { get; set; } = new();
    }

    public class UsuarioResumenResponse
    {
        public Guid UsuarioID { get; set; }
        public string Identificacion { get; set; } = null!;
        public string NombreCompleto { get; set; } = null!;
        public string NombreTipoIdent { get; set; } = null!;
        public string NombreEstado { get; set; } = null!;
        public string? EmailPrincipal { get; set; }
    }
}