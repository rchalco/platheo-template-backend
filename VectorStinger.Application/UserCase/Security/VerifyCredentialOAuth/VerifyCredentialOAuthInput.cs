using VectorStinger.Core.Domain.Managers.Security.AccountManager.VerifyCredentialOAuth;
using VectorStinger.Foundation.Abstractions.UserCase;

namespace VectorStinger.Application.UserCase.Security.VerifyCredentialOAuth
{
    public record VerifyCredentialOAuthInput : IUseCaseInput
    {
        public ProviderEnum Provider { get; set; } = ProviderEnum.none;
        public string Token { get; set; } = string.Empty;
    }
}
