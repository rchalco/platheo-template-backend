using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VectorStinger.Core.Domain.Managers.WebTemplate.GetQuestionsQuizHandle
{
    public record ResponseGetQuestionsQuizDTO
    {
        public int QuestionId { get; init; }
        public string QuestionText { get; init; } = string.Empty;
        public string QuestionType { get; init; } = string.Empty;
        public int IsClosed { get; init; }       
        public int IsActive { get; init; }
        public List<QuestionOptionDTO> Options { get; init; } = new();
    }

    public record QuestionOptionDTO
    {
        public int OptionId { get; init; }
        public string OptionText { get; init; } = string.Empty;
    }
}
