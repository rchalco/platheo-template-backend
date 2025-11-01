
using FluentResults;
using VectorStinger.Core.Domain.Managers.Security.AccountManager.ValidateToken;
using VectorStinger.Core.Domain.Managers.Security.AccountManager.VerifyCredential;
using VectorStinger.Core.Domain.Managers.Security.AccountManager.VerifyCredentialOAuth;

namespace VectorStinger.Core.Interfaces.Managers.Security
{
    public interface IAccountManager
    {
        Result<VerifyCredentialResponseDTO> VerifyCredentialAsync(VerifyCredentialRequestDTO request);
        Result<ValidateTokenResponse> ValidateTokenAsync(ValidateTokenRequest request);
        Task<Result<VerifyCredentialOAuthResponseDTO>> VerifyCredentialOAuthAsync(VerifyCredentialOAuthRequestDTO request);
    }
}
