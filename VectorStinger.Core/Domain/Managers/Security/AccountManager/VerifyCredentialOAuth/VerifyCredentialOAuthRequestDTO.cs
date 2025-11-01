using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VectorStinger.Core.Domain.Managers.Security.AccountManager.VerifyCredentialOAuth
{
    public record VerifyCredentialOAuthRequestDTO
    {
        public ProviderEnum Provider { get; set; } = ProviderEnum.none;
        public string Token { get; set; } = string.Empty;
    }

    public enum ProviderEnum
    {
        none = 0,
        google = 1,
        facebook = 2,
        apple = 3
    }
}
