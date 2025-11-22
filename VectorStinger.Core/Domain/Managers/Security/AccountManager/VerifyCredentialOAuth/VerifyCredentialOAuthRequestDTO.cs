using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VectorStinger.Core.Domain.Managers.Security.AccountManager.VerifyCredentialOAuth
{
    public record VerifyCredentialOAuthRequestDTO(
        ProviderEnum Provider,
        string Token);

    public enum ProviderEnum
    {
        none = 0,
        google = 1,
        facebook = 2,
        apple = 3
    }
}
