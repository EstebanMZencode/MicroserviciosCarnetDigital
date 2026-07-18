using FluentValidation;
using MicroServicioUsuarios.Entities;
using MicroServicioUsuarios.Repository;
using MicroServicioUsuarios.Services.ExternalServices;

namespace MicroServicioUsuarios.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IValidator<UsuarioRequest> _validator;
        private readonly IAuthE_Service _authE_Service;
        private readonly ITipoIdentificacionE_Service _tipoIdentService;
        private readonly IInstitucionE_Service _institucionService;

        public UsuarioService(
            IUsuarioRepository usuarioRepository,
            IValidator<UsuarioRequest> validator,
            IAuthE_Service authE_Service,
            ITipoIdentificacionE_Service tipoIdentService,
            IInstitucionE_Service institucionService)
        {
            _usuarioRepository = usuarioRepository;
            _validator = validator;
            _authE_Service = authE_Service;
            _tipoIdentService = tipoIdentService;
            _institucionService = institucionService;
        }

        // Crear Usuario
        public async Task<IResult> CreateUsuarioServiceAsync(UsuarioRequest usuario, string token)
        {
            // Validación del token
            var tokenResult = await _authE_Service.ValidarTokenAsync(token);

            if (!tokenResult.IsSuccess)
            {
                return tokenResult.ToIResult();
            } 

            // Valida UsuarioRequest usando FluentValidation
            var validation = await ValidateRequestAsync(usuario);

            if (validation is not null)
            {
                return validation;
            }

            /**
             * Validacón de microservicios externos
             */
            var tipoIdentResult = await _tipoIdentService
                .ValidarTipoIdentificacionIDAsync(usuario.TipoIdentificacion, token);

            if (!tipoIdentResult.IsSuccess)
            {
                return tipoIdentResult.ToIResult();
            }

            // Tipo Usuario

            // Roles (no controlado en la db, solucionar revisar ese error)

            foreach (var perfil in usuario.Perfiles)
            {
                var institucionesResult = await _institucionService.ValidarInstitucionIDAsync(perfil.InstitucionID, token);

                if (!institucionesResult.IsSuccess)
                {
                    return institucionesResult.ToIResult();
                }

                var dominiosList = institucionesResult.Data; // Necesito los dominios para validar correos
            }

            // Carreras

            // Areas Trabajo

            // Crea el usuario en la Base de Datos
            await _usuarioRepository.CrearUsuarioDBAsync(usuario);

            return MicroServicesResponse.Created(usuario).ToIResult(); // Cambiar el objeto
        }

        private async Task<IResult?> ValidateRequestAsync(UsuarioRequest usuario)
        {
            var result = await _validator.ValidateAsync(usuario);

            if (result.IsValid)
            {
                return null;
            }

            var errors = result.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray()
                );

            return MicroServicesResponse.BadRequest(errors: errors).ToIResult();
        }

    }
}

// TO-DO: Validar que los numeros de teléfono sean válidos y no estén duplicados en Base de Datos

// TO-DO: Validar que TipoUsuarioID exista (Microservicio Catálogos)  

// TO-DO: Validar que RolID exista y sea compatible con TipoUsuarioID

// TO-DO: Validar que CarrerasID existan y pertenezcan a la institución

// TO-DO: Validar que AreasTrabajoID existan y pertenezcan a la institución

// TO-DO: Validar condicional por tipo: Estudiante → carreras obligatorias, Funcionario → áreas obligatorias

// TO-DO: Validar que el email coincida con el dominio de la institución (Microservicio Instituciones) *

// TO-DO: Validar que email no exista ya en Login/EmailXUsuarios 