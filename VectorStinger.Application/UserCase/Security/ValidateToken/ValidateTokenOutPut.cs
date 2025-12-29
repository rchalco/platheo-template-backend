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
    public class ValidateTokenOutPut : IUseCaseOutput
    {
        [DataMember(Order = 1)]
        public bool IsValid { get; set; } = false;
        
        [DataMember(Order = 2)]
        public string Token { get; set; } = string.Empty;
        
        [DataMember(Order = 3)]
        public DateTime TimeExpired { get; set; } = DateTime.Now;
    }
}
