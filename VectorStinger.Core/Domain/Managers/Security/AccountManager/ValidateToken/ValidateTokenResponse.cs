using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VectorStinger.Core.Domain.Managers.Security.AccountManager.ValidateToken
{
    public record ValidateTokenResponse(
        bool IsValid,
        string Token,
        DateTime TimeExpired);
}
