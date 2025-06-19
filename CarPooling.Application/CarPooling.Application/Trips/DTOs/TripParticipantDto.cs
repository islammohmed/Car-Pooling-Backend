

namespace CarPooling.Application.Trips.DTOs
{
    public class TripParticipantDto
    {
        public int TripId { get; set; }
        public string UserId { get; set; }
        public int SeatCount { get; set; }
        public string Status { get; set; }
        public DateTime JoinedAt { get; set; }
    }
}
