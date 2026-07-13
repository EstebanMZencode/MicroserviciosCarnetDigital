using FluentValidation;

namespace MicroServicioUsuarios.Services.Validators
{
    public static class GuidValidationExtension
    {
        public static IRuleBuilderOptions<T, string> ValidGuid<T>(this IRuleBuilder<T, string> rule)
        {
            return rule.Must(id => Guid.TryParse(id, out _));
        }
    }
}
