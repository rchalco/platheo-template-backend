
using FluentResults;
using VectorStinger.Core.Domain.Managers.Security.AccountManager.ValidateToken;
using VectorStinger.Core.Domain.Managers.Security.AccountManager.VerifyCredentialOAuth;
using System.Threading.Tasks;

namespace VectorStinger.Core.Interfaces.Managers.Security
{
    public interface IAccountManager
    {
        Task<Result<ValidateTokenResponse>> ValidateTokenAsync(ValidateTokenRequest request);
        Task<Result<VerifyCredentialOAuthResponseDTO>> VerifyCredentialOAuthAsync(VerifyCredentialOAuthRequestDTO request);
    }
}
