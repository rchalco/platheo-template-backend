using System;
using System.Collections.Generic;

namespace VectorStinger.Core.Domain.DataBase.Models;

public partial class User
{
    public int UserId { get; set; }

    public string OauthId { get; set; } = null!;

    public string OauthProvider { get; set; } = null!;

    public string OauthToken { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<FakeDomain> FakeDomains { get; set; } = new List<FakeDomain>();

    public virtual ICollection<Session> Sessions { get; set; } = new List<Session>();

    public virtual ICollection<Template> Templates { get; set; } = new List<Template>();

    public virtual ICollection<UserAnswer> UserAnswers { get; set; } = new List<UserAnswer>();
}
