using System.Runtime.Serialization;

namespace CarPooling.Domain.Enums
{
    public enum TripStatus
    {
        [EnumMember(Value = "Pending")]
        Pending,

        [EnumMember(Value = "Confirmed")]
        Confirmed,

        [EnumMember(Value = "Ongoing")]
        Ongoing,

        [EnumMember(Value = "Completed")]
        Completed,

        [EnumMember(Value = "Cancelled")]
        Cancelled
    }
}
