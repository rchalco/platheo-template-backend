using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VectorStinger.Core.Domain.Managers.Security.AccountManager.VerifyCredentialOAuth
{
    public record VerifyCredentialOAuthResponseDTO
    {
        public bool IsValid { get; set; }
        public long IdUser { get; set; }
        public long IdSesion { get; set; }
        public string Token { get; set; } = string.Empty;
        public DateTime Expiration { get; set; }
        public string Message { get; set; } = string.Empty;
        public string NamePerson { get; set; } = string.Empty;
        public string PictureUrl { get; set; } = string.Empty;
    }
}
