using System.Runtime.Serialization;

namespace CarPooling.Domain.Enums
{
    public enum JoinStatus
    {
        [EnumMember(Value = "Pending")]
        Pending,

        [EnumMember(Value = "Confirmed")]
        Confirmed,

        [EnumMember(Value = "Cancelled")]
        Cancelled,

        [EnumMember(Value = "Rejected")]
        Rejected
    }
}
