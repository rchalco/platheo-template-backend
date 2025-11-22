namespace VectorStinger.Core.Domain.Managers.WebTemplate.RegisterQuestionsQuizHandle
{
    public record ResponseRegisterQuestionsQuizDTO
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public int TotalAnswersRegistered { get; set; }
    }
}
