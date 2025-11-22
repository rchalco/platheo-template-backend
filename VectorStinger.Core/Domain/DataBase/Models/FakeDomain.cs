using System;
using System.Collections.Generic;

namespace VectorStinger.Core.Domain.DataBase.Models;

public partial class FakeDomain
{
    public int DomainId { get; set; }

    public int UserId { get; set; }

    public int TemplateId { get; set; }

    public string DomainName { get; set; } = null!;

    public bool? IsAvailable { get; set; }

    public DateTime? ConfiguredAt { get; set; }

    public virtual Template Template { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
