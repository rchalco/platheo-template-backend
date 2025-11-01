using FluentValidation;
using VectorStinger.Foundation.Abstractions.UserCase;


namespace VectorStinger.Application.UserCase.Security.ValidateToken
{
    public class ValidateTokenValidation : UserCaseValidation<ValidateTokenInput>
    {
        public ValidateTokenValidation()
        {
            RuleFor(x => x.Token)
             .NotEmpty().WithMessage("El token es obligatorio.");
        }
    }
}
