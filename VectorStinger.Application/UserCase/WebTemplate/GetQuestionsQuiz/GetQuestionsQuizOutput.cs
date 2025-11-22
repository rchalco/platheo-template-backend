using VectorStinger.Foundation.Abstractions.UserCase;

namespace VectorStinger.Application.UserCase.WebTemplate.GetQuestionsQuiz
{
    public class GetQuestionsQuizOutput : IUseCaseOutput
    {
        public List<QuestionQuizItem> Questions { get; set; } = new();
    }

    public class QuestionQuizItem
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public string QuestionType { get; set; } = string.Empty;
        public int IsClosed { get; set; }
        public int IsActive { get; set; }
        public List<QuestionOptionOutput> Options { get; set; } = new();
    }

    public class QuestionOptionOutput
    {
        public int OptionId { get; set; }
        public string OptionText { get; set; } = string.Empty;
    }
}
