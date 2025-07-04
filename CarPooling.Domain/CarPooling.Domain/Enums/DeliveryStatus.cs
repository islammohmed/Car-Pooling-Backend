namespace CarPooling.Domain.Enums
{
    public enum DeliveryStatus
    {
        Pending,        // Initial state when created
        Accepted,       // Driver accepted the delivery
        InTransit,      // Driver picked up the item
        Delivered,      // Successfully delivered
        Cancelled,      // Cancelled by sender
        Rejected        // Rejected by driver
    }
} 