using FluentResults;
using Microsoft.Extensions.Logging;
using VectorStinger.Core.Interfaces.Managers.WebTemplate;
using VectorStinger.Foundation.Abstractions.UserCase;
using VectorStinger.Foundation.Utilities.Mapper;
using VectorStinger.Infrastructure.DataAccess.Interface;

namespace VectorStinger.Application.UserCase.WebTemplate.GetQuestionsQuiz
{
    public class GetQuestionsQuizUseCase : BaseUseCase<GetQuestionsQuizInput, GetQuestionsQuizOutput, GetQuestionsQuizValidation>
    {
        private readonly IWebTemplateManager _webTemplateManager;

        public GetQuestionsQuizUseCase(
            GetQuestionsQuizInput userCaseInput,
            GetQuestionsQuizValidation validationRules,
            IRepository repository,
            IWebTemplateManager webTemplateManager,
            ILogger<GetQuestionsQuizUseCase> logger)
            : base(userCaseInput, validationRules, repository, logger)
        {
            Description = "Obtiene todas las preguntas del quiz con sus opciones de respuesta";
            _webTemplateManager = webTemplateManager;
        }

        public override async Task<Result<GetQuestionsQuizOutput>> ExecuteBusinessAsync(GetQuestionsQuizInput input)
        {
            var result = _webTemplateManager.GetQuestionsQuizHandle();

            if (result.IsFailed)
            {
                return Result.Fail<GetQuestionsQuizOutput>(result.Errors);
            }

            var mappedQuestions = result.Value.Select(question =>
            {
                var questionItem = MapUtil.MapTo<Core.Domain.Managers.WebTemplate.GetQuestionsQuizHandle.ResponseGetQuestionsQuizDTO, QuestionQuizItem>(question);
                questionItem.Options = question.Options.Select(option =>
                    MapUtil.MapTo<Core.Domain.Managers.WebTemplate.GetQuestionsQuizHandle.QuestionOptionDTO, QuestionOptionOutput>(option)
                ).ToList();
                return questionItem;
            }).ToList();

            var output = new GetQuestionsQuizOutput
            {
                Questions = mappedQuestions
            };

            return await Task.FromResult(Result.Ok(output));
        }
    }
}
