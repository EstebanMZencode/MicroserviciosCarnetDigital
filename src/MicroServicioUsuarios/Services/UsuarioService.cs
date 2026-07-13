using FluentValidation;
using MicroServicioUsuarios.Entities;
using MicroServicioUsuarios.Repository;

namespace MicroServicioUsuarios.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly IValidator<UsuarioRequest> _validator;
        private readonly ExternalServices.IAuthE_Service _authE_Service;
        private readonly ExternalServices.ITipoIdentificacionE_Service _tipoIdentificacionE_Service;
        private readonly IUsuarioRepository _usuarioRepository;

        public UsuarioService(
            IValidator<UsuarioRequest> validator,
            ExternalServices.IAuthE_Service authE_Service,
            ExternalServices.ITipoIdentificacionE_Service tipoIdentificacionE_Service,
            IUsuarioRepository usuarioRepository)
        {
            _validator = validator;
            _authE_Service = authE_Service;
            _tipoIdentificacionE_Service = tipoIdentificacionE_Service;
            _usuarioRepository = usuarioRepository;
        }

        // Crear Usuario
        public async Task<IResult> CreateUsuarioServiceAsync(UsuarioRequest usuario, string token)
        {
            // Validación del token
            /*if (!await _authE_Service.ValidarTokenAsync(token))
            {
                return Results.Json(new 
                {
                    StatusCode = 401,
                    message = "Unauthorized"
                }, 
                statusCode: 401);
            } //comentado para pruebas*/

            // Valida UsuarioRequest usando FluentValidation
            var validationResult = await _validator.ValidateAsync(usuario);

            if (!validationResult.IsValid)
            {
                var errores = validationResult.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                       g => g.Key,
                       g => g.Select(e => e.ErrorMessage).ToArray()
                    );

                return Results.Json(new
                {
                    StatusCode = 400,
                    Message = "Bad Request",
                    Errors = errores
                }, statusCode: 400);

            }

            //Validaciones de microservicios externos
            var (tipoIdentificacionResult, tipoIdentificaccionError) = await _tipoIdentificacionE_Service
                .ValidarTipoIdentificacionAsync(usuario.TipoIdentificacion, token);

            if (tipoIdentificacionResult != ExternalServices.TipoIdentificacionE_Service.TipoIdentificacionResponse.Success)
            {
                return tipoIdentificacionResult switch
                {
                    // No Autorizado
                    ExternalServices.TipoIdentificacionE_Service.TipoIdentificacionResponse.Unauthorized => Results.Json(new
                    {
                        StatusCode = 401,
                        Message = "Unauthorized",
                        Errors = tipoIdentificaccionError
                    }, statusCode: 401),

                    // Inactivo 
                    ExternalServices.TipoIdentificacionE_Service.TipoIdentificacionResponse.Inactive => Results.Json(new
                    {
                        StatusCode = 400,
                        Message = "Bad Request",
                        Errors = tipoIdentificaccionError
                    }, statusCode: 400),

                    // No encontrado
                    ExternalServices.TipoIdentificacionE_Service.TipoIdentificacionResponse.NotFound => Results.Json(new
                    {
                        StatusCode = 404,
                        Message = "Not Found",
                        Errors = tipoIdentificaccionError
                    }, statusCode: 404),

                    // Error de Conexión
                    ExternalServices.TipoIdentificacionE_Service.TipoIdentificacionResponse.ServiceUnavailable => Results.Json(new
                    {
                        StatusCode = 503,
                        Message = "Service Unavailable",
                        Errors = tipoIdentificaccionError
                    }, statusCode: 503),



                    _ => Results.Json(new
                    {
                        StatusCode = 500,
                        Message = "Internal Server Error",
                        Errors = tipoIdentificaccionError
                    }, statusCode: 500)
                };
            }

            // Aquí iría la lógica para crear un usuario en la base de datos
            // * Devuelve el objeto que irá en data y * //
            await _usuarioRepository.CrearUsuarioDBAsync(usuario);

            return Results.Json(new
            {
                StatusCode = 201,
                Message = "Created",
                Data = usuario // Cambiar a response
            }, statusCode: 201);
        }
    }
}



/*
        private async Task<bool> ValidarTokenAsync(string token)
        {
            try
            {
                var validateUrl = _configuration["MicroServicios:AuthValidateUrl"]!;
                var client = _httpClientFactory.CreateClient();

                var request = new HttpRequestMessage(HttpMethod.Get, validateUrl);
                request.Headers.Add("token", token);

                var response = await client.SendAsync(request);
                if (!response.IsSuccessStatusCode) return false;

                var body = await response.Content.ReadAsStringAsync();
                return body.Contains("true", StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudo contactar al servicio de validacion de token.");
                return false;
            }
        }
*/