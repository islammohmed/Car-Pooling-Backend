using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using CarPooling.Domain.Enums;

namespace CarPooling.Domain.Entities
{
    public class Trip
    {
        [Key]
        [Range(0, 10000)]
        public int TripId { get; set; }

        [Required]
        public string DriverId { get; set; } = string.Empty;

        [ForeignKey(nameof(DriverId))]
        public User Driver { get; set; } = null!;

        [Required, Range(0, 10000)]
        public decimal PricePerSeat { get; set; }

        public TimeSpan EstimatedDuration { get; set; }

        [Required, Range(1, 50)]
        public int AvailableSeats { get; set; }

        [StringLength(500)]
        public string? TripDescription { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        [Required]
        public TripStatus Status { get; set; }

        [Required, StringLength(100)]
        public string SourceLocation { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string Destination { get; set; } = string.Empty;

        [Required]
        public DateTime StartTime { get; set; }

        public DateTime CreatedAt { get; set; }

        public Gender? GenderPreference { get; set; }

        public ICollection<TripParticipant> Participants { get; set; } = new List<TripParticipant>();

        // Delivery-related properties
        [Range(0, 100)]
        public float? MaxDeliveryWeight { get; set; }

        public bool AcceptsDeliveries { get; set; }

        public ICollection<DeliveryRequest> Deliveries { get; set; } = new List<DeliveryRequest>();


    }
}