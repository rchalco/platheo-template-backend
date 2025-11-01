using FluentValidation;
using VectorStinger.Foundation.Abstractions.UserCase;

namespace VectorStinger.Application.UserCase.Security.VerifyCredential
{
    public class VerifyCredentialValidation : UserCaseValidation<VerifyCredentialInput>
    {
        public VerifyCredentialValidation()
        {
            RuleFor(x => x.User)
             .NotEmpty().WithMessage("El usuario es obligatorio.")
             .MinimumLength(3).WithMessage("El usuario deben tener al menos 3 caracteres.")
             .MaximumLength(50).WithMessage("El usuario deben tener maximo 50 caracteres.");

            RuleFor(x => x.Password)
             .NotEmpty().WithMessage("El password es obligatorio.")
             .MinimumLength(3).WithMessage("El password debe tener al menos 3 caracteres.")
             .MaximumLength(50).WithMessage("El usuario deben tener maximo 50 caracteres.");

            RuleFor(x => x.VersionApplication)
             .NotEmpty().WithMessage("La version es obligatorio.")
             .MinimumLength(3).WithMessage("La version debe tener al menos 3 caracteres.")
             .MaximumLength(10).WithMessage("El usuario deben tener maximo 10 caracteres.");
        }
    }
}
