using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;


namespace CarPooling.Domain.Entities
{
    public class DeliveryRequest
    {
        [Key]
        public int Id { get; set; }
        [ Phone]
        public string ReceiverPhone { get; set; } = string.Empty;
        public string DropoffLocation { get; set; } = string.Empty;
        public string SourceLocation { get; set; } = string.Empty;
        public float Weight { get; set; }
        public string ItemDescription { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime DeliveryStartDate { get; set; }
        public DateTime DeliveryEndDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? AcceptedAt { get; set; }
        public DateTime? PickedUpAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public string? DeliveryNotes { get; set; }

        [ForeignKey("Sender")]
        public string SenderId { get; set; } = string.Empty;
        public int? TripId { get; set; }
        public User Sender { get; set; } = null!;
        public Trip? Trip { get; set; }
    }

}
