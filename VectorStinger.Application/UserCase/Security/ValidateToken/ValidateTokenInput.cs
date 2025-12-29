using VectorStinger.Foundation.Abstractions.UserCase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace VectorStinger.Application.UserCase.Security.ValidateToken
{
    [DataContract]
    public class ValidateTokenInput: IUseCaseInput
    {
        [DataMember(Order = 1)]
        public string Token { get; set; } = string.Empty;
    }
}
