using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VectorStinger.Core.Domain.Managers.Security.AccountManager.VerifyCredential
{
    public record  VerifyCredentialResponseDTO
    {
        public bool IsValid { get; set; }
        public string Token { get; set; } = string.Empty;
        public DateTime Expiration { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
