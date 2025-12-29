using VectorStinger.Foundation.Abstractions.UserCase;
using System.Runtime.Serialization;

namespace VectorStinger.Application.UserCase.WebTemplate.RegisterQuestionsQuiz
{
    [DataContract]
    public class RegisterQuestionsQuizOutput : IUseCaseOutput
    {
        [DataMember(Order = 1)]
        public bool IsSuccess { get; set; }
        
        [DataMember(Order = 2)]
        public string Message { get; set; } = string.Empty;
        
        [DataMember(Order = 3)]
        public int TotalAnswersRegistered { get; set; }
    }
}
