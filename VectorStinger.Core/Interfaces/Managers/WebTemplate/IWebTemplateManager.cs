using FluentResults;
using VectorStinger.Core.Domain.Managers.WebTemplate.GetQuestionsQuizHandle;
using VectorStinger.Core.Domain.Managers.WebTemplate.RegisterQuestionsQuizHandle;
using VectorStinger.Core.Domain.Managers.WebTemplate.RegisterTemplateWebHandle;

namespace VectorStinger.Core.Interfaces.Managers.WebTemplate
{
    public interface IWebTemplateManager
    {
        Result<ResoponseRegisterTemplateWebDTO> RegisterTemplateWebHandle(RequestRegisterTemplateWebDTO request);
        Result<List<ResponseGetQuestionsQuizDTO>> GetQuestionsQuizHandle();
        Result<ResponseRegisterQuestionsQuizDTO> RegisterQuestionsQuizHandle(RequestRegisterQuestionsQuizDTO request);
    }
}
