using System;
using System.Collections.Generic;

namespace VectorStinger.Core.Domain.DataBase.Models;

public partial class Session
{
    public int SessionId { get; set; }

    public int UserId { get; set; }

    public string Token { get; set; } = null!;

    public string Provider { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual User User { get; set; } = null!;
}
