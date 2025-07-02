using CarPooling.Domain.Enums;

namespace CarPooling.Application.Trips.DTOs
{
    public class CreateTripDto
    {
        public string DriverId { get; set; } = string.Empty;
        public decimal PricePerSeat { get; set; }
        public TimeSpan EstimatedDuration { get; set; }
        public int AvailableSeats { get; set; }
        public string Notes { get; set; } = string.Empty;
        public TripStatus Status { get; set; }
        public string SourceLocation { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public string TripDescription { get; set; } = string.Empty;
        public Gender GenderPreference { get; set; }
    }
} 