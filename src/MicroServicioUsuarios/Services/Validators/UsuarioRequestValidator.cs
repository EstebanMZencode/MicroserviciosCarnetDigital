using FluentValidation;
using MicroServicioUsuarios.Entities;

namespace MicroServicioUsuarios.Services.Validators
{
    public class UsuarioRequestValidator : AbstractValidator<UsuarioRequest>
    {
        public UsuarioRequestValidator()
        {
            // Validaciones de los datos personales del usuario
            RuleFor(x => x.TipoIdentificacion)
                .Cascade(CascadeMode.Stop)
                .NotNull().NotEmpty().WithMessage("El tipo de identificación es obligatorio.")
                .ValidGuid().WithMessage("El código del tipo de identificación no tiene un formato válido.");

            RuleFor(x => x.Identificacion)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("La identificación es obligatoria.")
                .MinimumLength(9).WithMessage("Mínimo 9 caracteres.")
                .MaximumLength(100).WithMessage("Máximo 100 caracteres.")
                .Matches(@"^[A-Za-z0-9]+$").WithMessage("Valores no válidos en la identificación. Solo se permiten letras y números.");

            RuleFor(x => x.NombreCompleto)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("El nombre completo es obligatorio.")
                .Matches(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$").WithMessage("El nombre completo solo debe contener letras.")
                .Must(nombre => !nombre.Contains("  ")).WithMessage("El nombre completo no debe contener espacios dobles.")
                .Must(n => !n.StartsWith(' ') && !n.EndsWith(' ')).WithMessage("El nombre completo no debe empezar ni terminar con espacios.")
                .Must(n => n.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries).Length >= 2).WithMessage("El nombre completo debe incluir al menos nombre y apellido.")
                .MaximumLength(255).WithMessage("El nombre completo no puede superar los 255 caracteres.");

            // Valiaciones de la lista de teléfonos
            When(x => x.Telefonos != null && x.Telefonos.Count > 0, () =>
            {
                // No debe existir números de teléfono duplicados
                RuleFor(x => x.Telefonos)
                    .Cascade(CascadeMode.Stop)
                    .Must(t => t == null || t.Count <= 3).WithMessage("No puede registrar más de 3 teléfonos.")
                    .Must(telefonos => telefonos.Distinct().Count() == telefonos.Count).WithMessage("No se permiten números de teléfono duplicados.")
                    .DependentRules(() =>
                    {
                        RuleForEach(x => x.Telefonos)
                            .Cascade(CascadeMode.Stop)
                            .Matches(@"^\d+$").WithMessage("El teléfono '{PropertyValue}' tiene un formato inválido. Solo se permiten números.")
                            .MinimumLength(8).WithMessage("El teléfono '{PropertyValue}' debe tener mínimo 8 números.");
                    });
            });

            // Validaciones de la lista de perfiles
            RuleFor(x => x.Perfiles)
                .Cascade(CascadeMode.Stop)
                .NotNull().WithMessage("Los perfiles de usuario son obligatorios.")
                .Must(p => p.Count > 0).WithMessage("Debe tener al menos un perfil de usuario.")
                .Must(p => p.Count <= 5).WithMessage("No puede tener más de 5 perfiles.");

            When(x => x.Perfiles != null && x.Perfiles.Count > 0, () =>
            {
                // Validación de cada perfil individual
                RuleForEach(x => x.Perfiles)
                    .SetValidator(new PerfilUsuarioRequestValidator());

                // Validación de combinación única de InstituciónID, TipoUsuarioID y RolID en los perfiles
                RuleFor(x => x.Perfiles)
                    .Must(perfiles =>
                    {
                        var combinacionesUnicas = perfiles
                            .Where(p => p.InstitucionID != string.Empty &&
                                        p.TipoUsuarioID != string.Empty &&
                                        p.RolID != string.Empty)
                            .Select(p => new { p.InstitucionID, p.TipoUsuarioID, p.RolID });

                        return combinacionesUnicas.Distinct().Count() == combinacionesUnicas.Count();

                    }).WithMessage("No se permiten perfiles duplicados con la misma combinación de Institución, Tipo de Usuario y Rol.");

                // Validación de correos electrónicos únicos en los perfiles
                RuleFor(x => x.Perfiles)
                    .Must(perfiles =>
                    {
                        var emails = perfiles
                            .Where(p => p.LoginData != null && !string.IsNullOrEmpty(p.LoginData.Email))
                            .Select(p => p.LoginData.Email.Trim().ToLowerInvariant());

                        return emails.Distinct().Count() == emails.Count();

                    }).WithMessage("Cada perfil debe registrar un correo electrónico único. No se permiten emails duplicados.");

            });

        }

    }

}

// En PerfilRequestValidator, después de los RuleFor básicos:

// TO-DO: Validar que TipoIdentificacion exista (MicroServicio Catálogos)
// TO-DO: Validar que los numeros de teléfono sean válidos y no estén duplicados en Base de Datos

// TO-DO: Validar que InstitucionID exista (Microservicio Instituciones)

// TO-DO: Validar que TipoUsuarioID exista (Microservicio Catálogos)  

// TO-DO: Validar que RolID exista y sea compatible con TipoUsuarioID

// TO-DO: Validar que CarrerasID existan y pertenezcan a la institución

// TO-DO: Validar que AreasTrabajoID existan y pertenezcan a la institución

// TO-DO: Validar condicional por tipo: Estudiante → carreras obligatorias, Funcionario → áreas obligatorias

// TO-DO: Validar que el email coincida con el dominio de la institución (Microservicio Instituciones)

// TO-DO: Validar que email no exista ya en Login/EmailXUsuarios
