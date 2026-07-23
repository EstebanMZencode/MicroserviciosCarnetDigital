namespace SitioAdministrativoCarnetDigital.Services.MicroServicioRoles
{
    // Envuelve el resultado de cualquier llamada a un microservicio.
    // Success indica si el HTTP fue 2XX; de lo contrario ErrorMessage
    // contiene el texto extraído del Body de la respuesta (4XX / 5XX).
    public class ApiResult<T>
    {
        public bool    Success      { get; set; }
        public T?      Data         { get; set; }
        public string? ErrorMessage { get; set; }
        public int     StatusCode   { get; set; }

        public static ApiResult<T> Ok(T data, int statusCode = 200)
            => new() { Success = true,  Data = data, StatusCode = statusCode };

        public static ApiResult<T> Fail(int statusCode, string message)
            => new() { Success = false, ErrorMessage = message, StatusCode = statusCode };
    }
}
