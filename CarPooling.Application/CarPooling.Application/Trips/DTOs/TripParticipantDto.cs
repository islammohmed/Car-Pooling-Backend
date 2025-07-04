using System.Text.Json.Serialization;
using CarPooling.Domain.Enums;

namespace CarPooling.Application.Trips.DTOs
{
    public class TripParticipantDto
    {
        public int TripId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int SeatCount { get; set; }
        
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public JoinStatus Status { get; set; }
        public DateTime JoinedAt { get; set; } = DateTime.Now;
    }
}
