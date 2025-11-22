using System;
using System.Collections.Generic;

namespace VectorStinger.Core.Domain.DataBase.Models;

public partial class Page
{
    public int PageId { get; set; }

    public int TemplateId { get; set; }

    public string PageType { get; set; } = null!;

    public virtual ICollection<EditableSection> EditableSections { get; set; } = new List<EditableSection>();

    public virtual Template Template { get; set; } = null!;
}
