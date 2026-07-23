namespace SitioAdministrativoCarnetDigital.Services.MicroServicioAuth
{
    // Wrapper de resultado idéntico al de MicroServicioRoles.
    // Cada carpeta de servicio tiene su propio para no crear dependencias cruzadas.
    public class ApiResult<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? ErrorMessage { get; set; }
        public int StatusCode { get; set; }

        public static ApiResult<T> Ok(T data, int statusCode = 200)
            => new() { Success = true, Data = data, StatusCode = statusCode };

        public static ApiResult<T> Fail(int statusCode, string message)
            => new() { Success = false, ErrorMessage = message, StatusCode = statusCode };
    }
}