namespace Car_Pooling.Data.Enums
{
    public enum JoinStatus
    {
        Pending,    // User has requested to join the trip
        Confirmed,  // User's request has been accepted by the driver
        Cancelled,  // User has cancelled their request to join the trip
        Rejected    // Driver has rejected the user's request to join the trip
    }
}
