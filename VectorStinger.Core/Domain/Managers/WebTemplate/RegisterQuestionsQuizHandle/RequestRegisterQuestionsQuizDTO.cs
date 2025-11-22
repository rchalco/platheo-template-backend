namespace VectorStinger.Core.Domain.Managers.WebTemplate.RegisterQuestionsQuizHandle
{
    public record RequestRegisterQuestionsQuizDTO
    {
        public int UserId { get; set; }
        public List<UserAnswerDTO> Answers { get; set; } = new();
    }

    public record UserAnswerDTO
    {
        public int QuestionId { get; set; }
        public string QuestionType { get; set; } = string.Empty;
        public string? AnswerText { get; set; }
        public int? OptionId { get; set; }
    }
}
