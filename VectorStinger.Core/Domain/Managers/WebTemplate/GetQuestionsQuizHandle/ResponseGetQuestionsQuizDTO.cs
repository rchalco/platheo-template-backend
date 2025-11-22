using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VectorStinger.Core.Domain.Managers.WebTemplate.GetQuestionsQuizHandle
{
    public class ResponseGetQuestionsQuizDTO
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public string QuestionType { get; set; } = string.Empty;
        public int IsClosed { get; set; }       
        public int IsActive { get; set; }
        public List<QuestionOptionDTO> Options { get; set; } = new();
    }

    public class QuestionOptionDTO
    {
        public int OptionId { get; set; }
        public string OptionText { get; set; } = string.Empty;
    }
}
