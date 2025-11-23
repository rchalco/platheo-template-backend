using FluentResults;
using Microsoft.Extensions.Logging;
using VectorStinger.Core.Domain.Managers.WebTemplate.RegisterQuestionsQuizHandle;
using VectorStinger.Core.Interfaces.Managers.WebTemplate;
using VectorStinger.Foundation.Abstractions.UserCase;
using VectorStinger.Foundation.Utilities.Mapper;
using VectorStinger.Infrastructure.DataAccess.Interface;

namespace VectorStinger.Application.UserCase.WebTemplate.RegisterQuestionsQuiz
{
    public class RegisterQuestionsQuizUseCase : BaseUseCase<RegisterQuestionsQuizInput, RegisterQuestionsQuizOutput, RegisterQuestionsQuizValidation>
    {
        private readonly IWebTemplateManager _webTemplateManager;

        public RegisterQuestionsQuizUseCase(
            RegisterQuestionsQuizInput userCaseInput,
            RegisterQuestionsQuizValidation validationRules,
            IRepository repository,
            IWebTemplateManager webTemplateManager,
            ILogger<RegisterQuestionsQuizUseCase> logger)
            : base(userCaseInput, validationRules, repository, logger)
        {
            Description = "Registra las respuestas del usuario a las preguntas del quiz";
            _webTemplateManager = webTemplateManager;
        }

        public override async Task<Result<RegisterQuestionsQuizOutput>> ExecuteBusinessAsync(RegisterQuestionsQuizInput input)
        {
            // Mapear el input a la request del manager
            var request = new RequestRegisterQuestionsQuizDTO
            {
                UserId = input.UserId,
                Answers = input.Answers.Select(a => new UserAnswerDTO
                {
                    QuestionId = a.QuestionId,
                    QuestionType = a.QuestionType,
                    AnswerText = a.AnswerText,
                    OptionId = a.OptionId
                }).ToList()
            };

            var result = await _webTemplateManager.RegisterQuestionsQuizHandle(request);

            if (result.IsFailed)
            {
                return Result.Fail<RegisterQuestionsQuizOutput>(result.Errors);
            }

            var output = MapUtil.MapTo<ResponseRegisterQuestionsQuizDTO, RegisterQuestionsQuizOutput>(result.Value);

            return await Task.FromResult(Result.Ok(output));
        }
    }
}
