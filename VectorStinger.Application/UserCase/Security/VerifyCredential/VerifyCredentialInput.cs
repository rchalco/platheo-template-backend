using VectorStinger.Foundation.Abstractions.UserCase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace VectorStinger.Application.UserCase.Security.VerifyCredential
{
    public class VerifyCredentialInput : IUseCaseInput
    {
        public string User { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string VersionApplication { get; set; } = string.Empty;
    }
}
