namespace SitioAdministrativoCarnetDigital.Services.MicroServicioFotografias
{
    // Wrapper de resultado idéntico al de otros módulos — self-contained por carpeta.
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
