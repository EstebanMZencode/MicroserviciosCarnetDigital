namespace MicroServicioAuth.Entities
{
    /// <summary>
    /// Deserializa la respuesta del <c>MicroServicioParametros</c> al consultar un parámetro.
    /// Mapea la tabla <c>Parametros</c> de la base de datos <c>Carnet_Config</c>.
    /// </summary>
    public class ParametroValor
    {
        /// <summary>Identificador del parámetro (ej. "JWT_TOKEN", "REFRESH"). Máx. 10 chars.</summary>
        public string Identificador { get; set; } = string.Empty;

        /// <summary>Valor del parámetro como cadena de texto. Se parsea a entero (minutos) para duraciones.</summary>
        public string Valor { get; set; } = string.Empty;
    }
}
