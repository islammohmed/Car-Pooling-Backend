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
        
        // Additional user details
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public float? Rating { get; set; }
        public string? ProfileImage { get; set; }
        
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public UserRole UserRole { get; set; }
        
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Gender? Gender { get; set; }
        
        // Flag to identify if this participant is the driver of the trip
        public bool IsDriver { get; set; }
    }
}
