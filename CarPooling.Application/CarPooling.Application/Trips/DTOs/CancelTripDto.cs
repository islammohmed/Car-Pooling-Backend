using System.ComponentModel.DataAnnotations;
using CarPooling.Domain.Enums;

namespace CarPooling.Application.Trips.DTOs
{
    public class CancelTripDto
    {
        [Required]
        public int TripId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public UserRole Role { get; set; }

        [Required]
        [StringLength(500, MinimumLength = 10)]
        public string CancellationReason { get; set; } = string.Empty;
    }
}
