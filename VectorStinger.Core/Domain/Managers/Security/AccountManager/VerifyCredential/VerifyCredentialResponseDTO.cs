using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VectorStinger.Core.Domain.Managers.Security.AccountManager.VerifyCredential
{
    public record VerifyCredentialResponseDTO(
        bool IsValid,
        string Token,
        DateTime Expiration,
        string Message);
}
