using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using CarPooling.Domain.Enums;

namespace CarPooling.Application.DTOs
{
    public class CreateDeliveryRequestDto
    {
        [Required]
        [Phone]
        public string ReceiverPhone { get; set; } = string.Empty;

        [Required]
        public string DropoffLocation { get; set; } = string.Empty;

        [Required]
        public string SourceLocation { get; set; } = string.Empty;

        [Required]
        [Range(0.1, 100)]
        public float Weight { get; set; }

        [Required]
        public string ItemDescription { get; set; } = string.Empty;

        [Required]
        [Range(0, 10000)]
        public decimal Price { get; set; }
        
        [Required]
        public DateTime DeliveryStartDate { get; set; }
        
        [Required]
        public DateTime DeliveryEndDate { get; set; }
    }

    public class DeliveryRequestResponseDto
    {
        public int Id { get; set; }
        public string ReceiverPhone { get; set; } = string.Empty;
        public string DropoffLocation { get; set; } = string.Empty;
        public string SourceLocation { get; set; } = string.Empty;
        public float Weight { get; set; }
        public string ItemDescription { get; set; } = string.Empty;
        public decimal Price { get; set; }
        
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public DeliveryStatus Status { get; set; }
        
        public string SenderId { get; set; } = string.Empty;
        public string SenderName { get; set; } = string.Empty;
        public string? SenderPhone { get; set; }
        public string? SenderProfileImage { get; set; }
        public double? SenderRating { get; set; }
        
        public int? TripId { get; set; }
        public string? DriverName { get; set; }
        public string? DriverId { get; set; }
        public string? DriverPhone { get; set; }
        public string? DriverProfileImage { get; set; }
        public double? DriverRating { get; set; }
        
        public DateTime? EstimatedDeliveryTime { get; set; }
        public TimeSpan? EstimatedDuration { get; set; }
        public DateTime DeliveryStartDate { get; set; }
        public DateTime DeliveryEndDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? AcceptedAt { get; set; }
        public DateTime? PickedUpAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        
        // Trip details if associated with a trip
        public string? TripSourceLocation { get; set; }
        public string? TripDestination { get; set; }
        public DateTime? TripStartTime { get; set; }
        public string? TripDescription { get; set; }
        public string? DeliveryNotes { get; set; }
    }

    public class UpdateDeliveryStatusDto
    {
        [Required]
        public string Status { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }
    
    public class SelectTripForDeliveryDto
    {
        [Required]
        public int TripId { get; set; }
        public string? DeliveryNotes { get; set; }
    }
} 