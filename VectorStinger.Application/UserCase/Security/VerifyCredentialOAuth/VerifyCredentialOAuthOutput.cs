using VectorStinger.Foundation.Abstractions.UserCase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace VectorStinger.Application.UserCase.Security.VerifyCredentialOAuth
{
    [DataContract]
    public record VerifyCredentialOAuthOutput : IUseCaseOutput
    {
        [DataMember(Order = 1)]
        public bool IsValid { get; set; }
        
        [DataMember(Order = 2)]
        public long IdUser { get; set; }
        
        [DataMember(Order = 3)]
        public long IdSession { get; set; }
        
        [DataMember(Order = 4)]
        public string Token { get; set; } = string.Empty;
        
        [DataMember(Order = 5)]
        public DateTime Expiration { get; set; }
        
        [DataMember(Order = 6)]
        public string Message { get; set; } = string.Empty;
        
        [DataMember(Order = 7)]
        public string NamePerson { get; set; } = string.Empty;
        
        [DataMember(Order = 8)]
        public string PictureUrl { get; set; } = string.Empty;
    }
}
