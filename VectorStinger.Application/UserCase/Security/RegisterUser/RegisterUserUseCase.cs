using VectorStinger.Core.Interfaces.Managers.Security;
using FluentResults;
using VectorStinger.Infrastructure.DataAccess.Interface;
using Microsoft.Extensions.Logging;
using VectorStinger.Foundation.Abstractions.UserCase;

namespace VectorStinger.Application.UserCase.Security.RegisterUser
{
    public class RegisterUserUseCase : BaseUseCase<RegisterUserInput, RegisterUserOutput, RegisterUserValidation>
    {
        private readonly IAccountManager _accountManager;

        public RegisterUserUseCase(RegisterUserInput userCaseInput,
            RegisterUserValidation validationRules, IRepository
            repository, IAccountManager accountManager
            , ILogger<RegisterUserUseCase> logger) :
            base(userCaseInput, validationRules, repository, logger)
        {
            Description = "Realiza el registro de un usuario";
            _accountManager = accountManager;
        }

        public override async Task<Result<RegisterUserOutput>> ExecuteBusinessAsync(RegisterUserInput input)
        {
            throw new NotImplementedException();
        }
    }
}
