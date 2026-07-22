using System.Text.Json.Serialization;

namespace SitioAdministrativoCarnetDigital.Services.MicroServicioTiposIdentificacion
{
    // Campos confirmados contra la respuesta real del microservicio (Postman):
    // { "tipoIdentID": "...", "nombreTipoIdent": "...", "estado": true,
    //   "fechaCreacion": "...", "fechaModificacion": "..." }
    public class TipoIdentificacionDto
    {
        [JsonPropertyName("tipoIdentID")]
        public Guid TipoIdentificacionID { get; set; }

        [JsonPropertyName("nombreTipoIdent")]
        public string Nombre { get; set; } = string.Empty;

        [JsonPropertyName("estado")]
        public bool Estado { get; set; } = true;
    }

    public interface ITiposIdentificacionApiClient
    {
        void SetToken(string token);
        Task<List<TipoIdentificacionDto>> ObtenerTodosAsync();
        Task CrearAsync(string nombre);
        Task ActualizarAsync(Guid id, string nombre);
        Task EliminarAsync(Guid id);
    }
}