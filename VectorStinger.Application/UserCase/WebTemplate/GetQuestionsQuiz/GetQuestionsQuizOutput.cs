using VectorStinger.Foundation.Abstractions.UserCase;
using System.Runtime.Serialization;

namespace VectorStinger.Application.UserCase.WebTemplate.GetQuestionsQuiz
{
    [DataContract]
    public class GetQuestionsQuizOutput : IUseCaseOutput
    {
        [DataMember(Order = 1)]
        public List<QuestionQuizItem> Questions { get; set; } = new();
    }

    [DataContract]
    public class QuestionQuizItem
    {
        [DataMember(Order = 1)]
        public int QuestionId { get; set; }
        
        [DataMember(Order = 2)]
        public string QuestionText { get; set; } = string.Empty;
        
        [DataMember(Order = 3)]
        public string QuestionType { get; set; } = string.Empty;
        
        [DataMember(Order = 4)]
        public int IsClosed { get; set; }
        
        [DataMember(Order = 5)]
        public int IsActive { get; set; }
        
        [DataMember(Order = 6)]
        public List<QuestionOptionOutput> Options { get; set; } = new();
    }

    [DataContract]
    public class QuestionOptionOutput
    {
        [DataMember(Order = 1)]
        public int OptionId { get; set; }
        
        [DataMember(Order = 2)]
        public string OptionText { get; set; } = string.Empty;
    }
}
