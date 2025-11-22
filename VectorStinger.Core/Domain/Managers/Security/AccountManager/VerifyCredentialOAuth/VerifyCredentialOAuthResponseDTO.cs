using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VectorStinger.Core.Domain.Managers.Security.AccountManager.VerifyCredentialOAuth
{
    public record VerifyCredentialOAuthResponseDTO(
        bool IsValid,
        long IdUser,
        long IdSesion,
        string Token,
        DateTime Expiration,
        string Message,
        string NamePerson,
        string PictureUrl);
}
