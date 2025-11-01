using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VectorStinger.Core.Domain.Managers.Security.AccountManager.ValidateToken
{
    public record ValidateTokenResponse
    {
        public bool IsValid { get; set; } = false;
        public string Token { get; set; } = string.Empty;
        public DateTime TimeExpired { get; set; } = DateTime.Now;
    }
}
