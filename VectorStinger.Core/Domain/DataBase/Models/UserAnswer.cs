using System;
using System.Collections.Generic;

namespace VectorStinger.Core.Domain.DataBase.Models;

public partial class UserAnswer
{
    public int AnswerId { get; set; }

    public int UserId { get; set; }

    public int QuestionId { get; set; }

    public string? AnswerText { get; set; }

    public int? OptionId { get; set; }

    public DateTime? AnsweredAt { get; set; }

    public virtual QuestionOption? Option { get; set; }

    public virtual Question Question { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
