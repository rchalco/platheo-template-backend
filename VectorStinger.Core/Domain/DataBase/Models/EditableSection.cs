using System;
using System.Collections.Generic;

namespace VectorStinger.Core.Domain.DataBase.Models;

public partial class EditableSection
{
    public int SectionId { get; set; }

    public int PageId { get; set; }

    public string SectionType { get; set; } = null!;

    public string? ContentSection { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Page Page { get; set; } = null!;
}
