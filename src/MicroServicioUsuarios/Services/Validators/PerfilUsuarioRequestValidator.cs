using FluentValidation;
using MicroServicioUsuarios.Entities;

namespace MicroServicioUsuarios.Services.Validators
{
    public class PerfilUsuarioRequestValidator : AbstractValidator<PerfilUsuarioRequest>
    {
        public PerfilUsuarioRequestValidator()
        {

            RuleFor(x => x.InstitucionID)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("La institución es obligatoria.")
                .ValidGuid().WithMessage("El código de la institución no tiene un formato válido.");

            RuleFor(x => x.TipoUsuarioID)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("El tipo de usuario es obligatorio.")
                .ValidGuid().WithMessage("El código del tipo de usuario no tiene un formato válido.");

            RuleFor(x => x.RolID)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("El rol es obligatorio.")
                .ValidGuid().WithMessage("El código del rol no tiene un formato válido.");

            // Validación de la lista de carreras asociadas al perfil (solo para estudiantes)
            When(x => x.CarrerasID != null && x.CarrerasID.Count > 0, () =>
            {
                RuleForEach(x => x.CarrerasID)
                    .Cascade(CascadeMode.Stop)
                    .NotEmpty().WithMessage("El ID de la carrera proporcionado no es válido.")
                    .ValidGuid().WithMessage("El código de la carrera no tiene un formato válido.");

                RuleFor(x => x.CarrerasID)
                    .Must(carreras => carreras.Distinct().Count() == carreras.Count)
                    .WithMessage("No se permiten carreras duplicadas dentro del mismo perfil.");
            });

            // Validación de la lista de áreas de trabajo asociadas al perfil (solo para funcionarios)
            When(x => x.AreasTrabajoID != null && x.AreasTrabajoID.Count > 0, () =>
            {
                RuleForEach(x => x.AreasTrabajoID)
                    .Cascade(CascadeMode.Stop)
                    .NotEmpty().WithMessage("El ID del área de trabajo proporcionado no es válido.")
                    .ValidGuid().WithMessage("El código del área de trabajo no tiene un formato válido.");

                RuleFor(x => x.AreasTrabajoID)
                    .Must(areas => areas.Distinct().Count() == areas.Count)
                    .WithMessage("No se permiten áreas de trabajo duplicadas dentro del mismo perfil.");
            });

            // Validación para asegurar que un perfil no tenga simultáneamente carreras y áreas de trabajo
            RuleFor(x => x)
                .Must(x => !(x.CarrerasID != null && x.CarrerasID.Count > 0 &&
                             x.AreasTrabajoID != null && x.AreasTrabajoID.Count > 0))
                .WithMessage("Un perfil no puede tener carreras y áreas de trabajo simultáneamente.")
                .When(x => x.CarrerasID != null || x.AreasTrabajoID != null);

            // Validación para asegurar que el perfil tenga al menos carreras o áreas de trabajo
            RuleFor(x => x)
                .Must(x => (x.CarrerasID != null && x.CarrerasID.Count > 0) ||
                           (x.AreasTrabajoID != null && x.AreasTrabajoID.Count > 0))
                .WithMessage("El perfil debe tener al menos una carrera o un área de trabajo asignada.");

            // Validación de los datos necesarios para login
            RuleFor(x => x.LoginData)
                .NotNull().WithMessage("Los datos de inicio de sesión del perfil son obligatorios.")
                .SetValidator(new LoginUsuarioRequestValidator());
        }
    }
}
