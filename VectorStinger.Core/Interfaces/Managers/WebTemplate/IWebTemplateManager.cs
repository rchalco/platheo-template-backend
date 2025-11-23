using FluentResults;
using VectorStinger.Core.Domain.Managers.WebTemplate.GetQuestionsQuizHandle;
using VectorStinger.Core.Domain.Managers.WebTemplate.RegisterQuestionsQuizHandle;
using VectorStinger.Core.Domain.Managers.WebTemplate.RegisterTemplateWebHandle;

namespace VectorStinger.Core.Interfaces.Managers.WebTemplate
{
    public interface IWebTemplateManager
    {
        Task<Result<List<ResponseGetQuestionsQuizDTO>>> GetQuestionsQuizHandle();
        Task<Result<ResponseRegisterQuestionsQuizDTO>> RegisterQuestionsQuizHandle(RequestRegisterQuestionsQuizDTO request);
    }
}
