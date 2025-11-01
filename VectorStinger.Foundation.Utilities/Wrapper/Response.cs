using System;
using System.Collections;
using System.Runtime.Serialization;
using System.Collections.Generic;

namespace VectorStinger.Foundation.Utilities.Wrapper
{
    [DataContract]
    public enum ResponseType
    {
        [EnumMember(Value = "None")]
        None = 0,

        [EnumMember(Value = "Success")]
        Success = 1,

        [EnumMember(Value = "Warning")]
        Warning = 2,

        [EnumMember(Value = "Error")]
        Error = 3,

        [EnumMember(Value = "NoData")]
        NoData = 4
    }


    [DataContract]
    [Serializable]
    public class Response
    {
        public Response()
        {
            State = ResponseType.None;
        }
        [DataMember]
        public ResponseType State { get; set; }

        [DataMember]
        public string Message { get; set; }
        [DataMember]
        public string Code { get; set; }
    }

    [DataContract(IsReference = true)]
    [Serializable]
    public class ResponseQuery : Response
    {
        [DataMember]
        public IList ListEntities { get; set; }
    }

    [DataContract]
    [Serializable]
    public class ResponseQuery<T> : Response where T : class
    {
        [DataMember]
        public List<T> ListEntities { get; set; }
    }

    [DataContract]
    [Serializable]
    public class ResponseObject<T> : Response
    {
        [DataMember]
        public T Data { get; set; }
    }

}