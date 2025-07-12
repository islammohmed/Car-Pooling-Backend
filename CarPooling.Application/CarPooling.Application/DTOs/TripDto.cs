using CarPooling.Domain.Enums;
using System.Text.Json.Serialization;

namespace CarPooling.Application.DTOs
{
    public class CreateTripDto
    {
        public string DriverId { get; set; } = string.Empty;
        public decimal PricePerSeat { get; set; }
        public TimeSpan EstimatedDuration { get; set; }
        public int AvailableSeats { get; set; }
        public string Notes { get; set; } = string.Empty;

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public TripStatus Status { get; set; }
        public string SourceLocation { get; set; } = string.Empty;

        public double SourceLatitude { get; set; }

        public double SourceLongitude { get; set; }
        public string SourceCity { get; set; } = string.Empty;
        public string DestinationLocation { get; set; } = string.Empty;
        public string DestinationCity { get; set; } = string.Empty;

        public double DestinationLatitude { get; set; }
        public double DestinationLongitude { get; set; }
        //public string Destination { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public string TripDescription { get; set; } = string.Empty;

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Gender GenderPreference { get; set; }

        // Delivery-related properties
        public bool AcceptsDeliveries { get; set; }
        public float? MaxDeliveryWeight { get; set; }
    }
} 