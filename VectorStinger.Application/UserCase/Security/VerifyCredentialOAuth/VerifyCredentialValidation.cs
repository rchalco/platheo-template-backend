using FluentValidation;
using VectorStinger.Foundation.Abstractions.UserCase;

namespace VectorStinger.Application.UserCase.Security.VerifyCredentialOAuth
{
    public class VerifyCredentialOAuthValidation : UserCaseValidation<VerifyCredentialOAuthInput>
    {
        public VerifyCredentialOAuthValidation()
        {
            RuleFor(x => x.Token)
             .NotEmpty().WithMessage("El Token es obligatorio.");

            RuleFor(x => x.Provider)
                .NotEmpty().WithMessage("El proveedor es obligatorio.")
                .IsInEnum().WithMessage("El proveedor debe ser uno de los siguientes: google, facebook, apple.");

        }
    }
}
