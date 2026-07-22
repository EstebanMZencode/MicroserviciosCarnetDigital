using System.Text.Json.Serialization;

namespace SitioAdministrativoCarnetDigital.Services.MicroServicioTiposUsuario
{
    // Campos confirmados contra la respuesta real del microservicio (Postman):
    // { "tipoUsuarioID": "...", "nombreTipoUsuario": "...", "fechaCreacion": "...",
    //   "fechaModificacion": "...", "estado": true }
    public class TipoUsuarioDto
    {
        [JsonPropertyName("tipoUsuarioID")]
        public Guid TipoUsuarioID { get; set; }

        [JsonPropertyName("nombreTipoUsuario")]
        public string Nombre { get; set; } = string.Empty;

        [JsonPropertyName("estado")]
        public bool Estado { get; set; } = true;
    }

    public interface ITiposUsuarioApiClient
    {
        void SetToken(string token);
        Task<List<TipoUsuarioDto>> ObtenerTodosAsync();
        Task<TipoUsuarioDto> CrearAsync(string nombre);
        Task<TipoUsuarioDto> ActualizarAsync(Guid id, string nombre);
        Task EliminarAsync(Guid id);
    }
}