using System.ComponentModel.DataAnnotations;

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
        public string Status { get; set; } = string.Empty;
        public string SenderId { get; set; } = string.Empty;
        public string SenderName { get; set; } = string.Empty;
        public int? TripId { get; set; }
        public string? DriverName { get; set; }
        public DateTime? EstimatedDeliveryTime { get; set; }
    }

    public class UpdateDeliveryStatusDto
    {
        [Required]
        public string Status { get; set; } = string.Empty;
    }
} 