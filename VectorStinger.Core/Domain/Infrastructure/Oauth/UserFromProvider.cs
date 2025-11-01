using System;
using System.Collections.Generic;

namespace VectorStinger.Core.Domain.Infrastructure.Oauth
{
    public class UserFromProvider
    {
        public string Iss { get; set; } = string.Empty;
        public string Aud { get; set; } = string.Empty;
        public long AuthTime { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Sub { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool EmailVerified { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Picture { get; set; } = string.Empty;
        public ProviderInfo ProviderInfo { get; set; } = new ProviderInfo();
    }

    public class ProviderInfo
    {
        public Dictionary<string, List<string>> Identities { get; set; } = new();
        public string SignInProvider { get; set; } = string.Empty;
    }
}