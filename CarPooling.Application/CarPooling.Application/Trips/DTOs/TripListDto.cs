using CarPooling.Domain.Enums;
using System.Text.Json.Serialization;

namespace CarPooling.Application.Trips.DTOs
{
    public class TripListDto
    {
        public int TripId { get; set; }
        public string DriverName { get; set; } = string.Empty;
        public decimal PricePerSeat { get; set; }
        public TimeSpan EstimatedDuration { get; set; }
        public int AvailableSeats { get; set; }
        public string? TripDescription { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public TripStatus Status { get; set; }
        
        public string SourceLocation { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime CreatedAt { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Gender? GenderPreference { get; set; }
        
        public int ParticipantsCount { get; set; }
    }
} 