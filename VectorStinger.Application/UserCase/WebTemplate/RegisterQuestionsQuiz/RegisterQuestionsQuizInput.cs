using VectorStinger.Foundation.Abstractions.UserCase;
using System.Runtime.Serialization;

namespace VectorStinger.Application.UserCase.WebTemplate.RegisterQuestionsQuiz
{
    [DataContract]
    public class RegisterQuestionsQuizInput : IUseCaseInput
    {
        [DataMember(Order = 1)]
        public int UserId { get; set; }
        
        [DataMember(Order = 2)]
        public List<UserAnswerInput> Answers { get; set; } = new();
    }

    [DataContract]
    public class UserAnswerInput
    {
        [DataMember(Order = 1)]
        public int QuestionId { get; set; }
        
        [DataMember(Order = 2)]
        public string QuestionType { get; set; } = string.Empty;
        
        [DataMember(Order = 3)]
        public string? AnswerText { get; set; }
        
        [DataMember(Order = 4)]
        public int? OptionId { get; set; }
    }
}
