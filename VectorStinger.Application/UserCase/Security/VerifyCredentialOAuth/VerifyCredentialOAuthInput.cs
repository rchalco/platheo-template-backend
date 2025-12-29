using VectorStinger.Core.Domain.Managers.Security.AccountManager.VerifyCredentialOAuth;
using VectorStinger.Foundation.Abstractions.UserCase;
using System.Runtime.Serialization;

namespace VectorStinger.Application.UserCase.Security.VerifyCredentialOAuth
{
    [DataContract]
    public record VerifyCredentialOAuthInput : IUseCaseInput
    {
        [DataMember(Order = 1)]
        public ProviderEnum Provider { get; set; } = ProviderEnum.none;
        
        [DataMember(Order = 2)]
        public string Token { get; set; } = string.Empty;
    }
}
