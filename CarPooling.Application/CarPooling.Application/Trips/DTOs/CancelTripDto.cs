using System.ComponentModel.DataAnnotations;

namespace CarPooling.Application.Trips.DTOs
{
    public class CancelTripDto
    {
        [Required]
        public int TripId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        public string? CancellationReason { get; set; }
    }
}
