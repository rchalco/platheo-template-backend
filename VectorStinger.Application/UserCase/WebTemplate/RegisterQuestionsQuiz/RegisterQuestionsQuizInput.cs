using VectorStinger.Foundation.Abstractions.UserCase;

namespace VectorStinger.Application.UserCase.WebTemplate.RegisterQuestionsQuiz
{
    public class RegisterQuestionsQuizInput : IUseCaseInput
    {
        public int UserId { get; set; }
        public List<UserAnswerInput> Answers { get; set; } = new();
    }

    public class UserAnswerInput
    {
        public int QuestionId { get; set; }
        public string QuestionType { get; set; } = string.Empty;
        public string? AnswerText { get; set; }
        public int? OptionId { get; set; }
    }
}
