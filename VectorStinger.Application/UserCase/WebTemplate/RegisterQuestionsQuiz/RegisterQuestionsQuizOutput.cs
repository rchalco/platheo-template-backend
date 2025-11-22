using VectorStinger.Foundation.Abstractions.UserCase;

namespace VectorStinger.Application.UserCase.WebTemplate.RegisterQuestionsQuiz
{
    public class RegisterQuestionsQuizOutput : IUseCaseOutput
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public int TotalAnswersRegistered { get; set; }
    }
}
