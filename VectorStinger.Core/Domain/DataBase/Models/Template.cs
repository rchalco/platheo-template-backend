using System;
using System.Collections.Generic;

namespace VectorStinger.Core.Domain.DataBase.Models;

public partial class Template
{
    public int TemplateId { get; set; }

    public int UserId { get; set; }

    public string TemplateFileKey { get; set; } = null!;

    public string TemplateUrl { get; set; } = null!;

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<FakeDomain> FakeDomains { get; set; } = new List<FakeDomain>();

    public virtual ICollection<Page> Pages { get; set; } = new List<Page>();

    public virtual User User { get; set; } = null!;
}
