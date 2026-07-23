using System.Text.Json.Serialization;

namespace SitioAdministrativoCarnetDigital.Services.MicroServicioRoles
{
    // ── Respuesta paginada del GET /api/rol ─────────────────────────────────
    public class RolPaginadoDto
    {
        [JsonPropertyName("totalRegistros")]
        public int TotalRegistros { get; set; }

        [JsonPropertyName("totalPaginas")]
        public int TotalPaginas { get; set; }

        [JsonPropertyName("pagina")]
        public int Pagina { get; set; }

        [JsonPropertyName("tamano")]
        public int Tamano { get; set; }

        [JsonPropertyName("roles")]
        public List<RolDto> Roles { get; set; } = new();
    }

    // Rol individual con su lista de pantallas asignadas.
    public class RolDto
    {
        [JsonPropertyName("rolID")]
        public Guid RolID { get; set; }

        [JsonPropertyName("nombreRol")]
        public string NombreRol { get; set; } = string.Empty;

        [JsonPropertyName("pantallas")]
        public List<Guid> Pantallas { get; set; } = new();
    }

    // Pantalla tal como viene embebida en la respuesta de MicroServicioRoles.
    public class PantallaRolDto
    {
        [JsonPropertyName("pantallaID")]
        public Guid PantallaID { get; set; }

        [JsonPropertyName("nombrePantalla")]
        public string NombrePantalla { get; set; } = string.Empty;

        [JsonPropertyName("descripcion")]
        public string Descripcion { get; set; } = string.Empty;

        [JsonPropertyName("ruta")]
        public string Ruta { get; set; } = string.Empty;
    }

    // ── Respuesta del GET /pantallas de MicroServicioPantallas ──────────────
    // Formato real confirmado: { pageNumber, pageSize, total, items: [...] }
    public class PantallasPaginadoDto
    {
        [JsonPropertyName("pageNumber")]
        public int PageNumber { get; set; }

        [JsonPropertyName("pageSize")]
        public int PageSize { get; set; }

        [JsonPropertyName("total")]
        public int Total { get; set; }

        // Lista de pantallas bajo la clave "items".
        [JsonPropertyName("items")]
        public List<PantallaDto> Items { get; set; } = new();
    }

    // Pantalla del sistema tal como la devuelve MicroServicioPantallas.
    public class PantallaDto
    {
        [JsonPropertyName("pantallaID")]
        public Guid PantallaID { get; set; }

        [JsonPropertyName("nombrePantalla")]
        public string NombrePantalla { get; set; } = string.Empty;

        [JsonPropertyName("descripcion")]
        public string Descripcion { get; set; } = string.Empty;

        [JsonPropertyName("ruta")]
        public string Ruta { get; set; } = string.Empty;

        [JsonPropertyName("estado")]
        public bool Estado { get; set; }
    }
}
