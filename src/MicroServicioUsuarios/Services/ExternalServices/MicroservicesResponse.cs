namespace MicroServicioUsuarios.Services.ExternalServices
{

    public record MicroServicesResponse(
        int StatusCode,
        string Message,
        bool IsSuccess,
        object? Data = null,
        object? Errors = null
    )

    {
        public static MicroServicesResponse Success(object? data = null)
            => new(200, "OK", true, data);

        public static MicroServicesResponse Created(object data)
            => new(201, "Created", true, Data: data);

        public static MicroServicesResponse BadRequest(object? errors = null)
            => new(400, "Bad Request", false, Errors: errors);

        public static MicroServicesResponse Unauthorized(object? errors = null)
            => new(401, "Unauthorized", false, Errors: errors);

        public static MicroServicesResponse NotFound(object? errors = null)
            => new(404, "Not Found", false, Errors: errors);

        public static MicroServicesResponse ServiceUnavailable(object? errors = null)
            => new(503, "Service Unavailable", false, Errors: errors);

        public static MicroServicesResponse InternalServerError(object? errors = null)
            => new(500, "Internal Server Error", false, Errors: errors);

        public IResult ToIResult()
            => Results.Json(this, statusCode: StatusCode);
    }
}