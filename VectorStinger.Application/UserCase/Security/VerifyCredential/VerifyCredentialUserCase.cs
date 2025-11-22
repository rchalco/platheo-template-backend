using FluentResults;
using Microsoft.Extensions.Logging;
using VectorStinger.Core.Domain.Managers.Security.AccountManager.VerifyCredential;
using VectorStinger.Core.Interfaces.Managers.Security;
using VectorStinger.Foundation.Abstractions.UserCase;
using VectorStinger.Infrastructure.DataAccess.Interface;

namespace VectorStinger.Application.UserCase.Security.VerifyCredential
{
    public class VerifyCredentialUseCase : BaseUseCase<VerifyCredentialInput, VerifyCredentialOutput, VerifyCredentialValidation>
    {
        private readonly IAccountManager _accountManager;

        public VerifyCredentialUseCase(VerifyCredentialInput userCaseInput,
            VerifyCredentialValidation validationRules,
            IRepository repository,
            IAccountManager accountManager,
             ILogger<VerifyCredentialUseCase> logger)
            : base(userCaseInput, validationRules, repository, logger)
        {
            Description = "Valida la credencial del administrador del sistema";
            _accountManager = accountManager;
        }

        public override async Task<Result<VerifyCredentialOutput>> ExecuteBusinessAsync(VerifyCredentialInput input)
        {
            var resultAuthentication =  _accountManager.VerifyCredentialAsync(new VerifyCredentialRequestDTO(
                input.User,
                input.Password,
                input.VersionApplication));

            if (resultAuthentication.IsFailed)
            {
                return Result.Fail<VerifyCredentialOutput>(resultAuthentication.Errors);
            }

            var verifyCredentialOutput = new VerifyCredentialOutput
            {
                IsValid = true,
                Token = resultAuthentication.Value.Token,
                Expiration = resultAuthentication.Value.Expiration,
                Message = resultAuthentication.Value.Message
            };

            return Result.Ok(verifyCredentialOutput);
        }
    }
}
