using VectorStinger.Foundation.Abstractions.UserCase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VectorStinger.Application.UserCase.Security.VerifyCredential
{
    public class VerifyCredentialOutput: IUseCaseOutput
    {
        public bool IsValid { get; set; } = false;
        public string Token { get; set; } = string.Empty;
        public DateTime Expiration { get; set; }
        public string Message { get; set; } = string.Empty;

    }
}
