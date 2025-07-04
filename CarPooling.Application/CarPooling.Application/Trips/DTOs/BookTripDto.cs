using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using CarPooling.Domain.Enums;

namespace CarPooling.Application.Trips.DTOs
{
    public class BookTripDto
    {
        public int TripId { get; set; }

        [Required(ErrorMessage = "UserId is required.")]
        public string UserId { get; set; } = string.Empty;
   
        [Required(ErrorMessage = "SeatCount is required.")]
        public int SeatCount { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public JoinStatus Status { get; set; }
        public DateTime JoinedAt { get; set; } = DateTime.Now;
    }
}
