using System.Runtime.Serialization;

namespace ConsoleTestZeroMQ
{


    [DataContract]
    public class ValidateTokenInput 
    {
        [DataMember(Order = 1)]
        public string Token { get; set; } = string.Empty;
    }
}
