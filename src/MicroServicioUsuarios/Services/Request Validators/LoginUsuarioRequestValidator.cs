using FluentValidation;
using MicroServicioUsuarios.Entities;

namespace MicroServicioUsuarios.Services.Validators
{
    public class LoginUsuarioRequestValidator : AbstractValidator<LoginUsuarioRequest>
    {
        public LoginUsuarioRequestValidator()
        {
            RuleFor(x => x.Email)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("El correo electrónico del perfil es obligatorio.")
                .EmailAddress().WithMessage("El formato del correo electrónico ('{PropertyValue}') no es válido.")
                .MaximumLength(255).WithMessage("El correo no puede exceder los 255 caracteres.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("La contraseña es obligatoria.")
                .MinimumLength(8).WithMessage("Mínimo 8 caracteres.")
                .MaximumLength(255).WithMessage("La contraseña es demásiado larga.")
                .Must(p => !p.Contains(' ')).WithMessage("La contraseña no debe contener espacios.")
                .Matches(@"[A-Z]").WithMessage("Debe contener al menos una mayúscula.")
                .Matches(@"[a-z]").WithMessage("Debe contener al menos una minúscula.")
                .Matches(@"[0-9]").WithMessage("Debe contener al menos un número.")
                .Matches(@"[^A-Za-z0-9\s]").WithMessage("Debe contener al menos un carácter especial.");
        }
    }
}
