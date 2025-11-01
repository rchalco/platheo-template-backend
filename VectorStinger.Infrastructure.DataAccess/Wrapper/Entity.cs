using System.Runtime.Serialization;

namespace VectorStinger.Infrastructure.DataAccess.Wrapper
{
    [DataContract]
    public enum StateEntity
    {
        [EnumMember]
        none = 0,
        [EnumMember]
        add = 1,
        [EnumMember]
        modify = 2,
        [EnumMember]
        remove = 3
    }
    [DataContract]
    public class Entity<T> where T : class, new()
    {
        [DataMember]
        public StateEntity stateEntity { get; set; }
        [DataMember]
        public T EntityDB { get; set; }
    }
}
