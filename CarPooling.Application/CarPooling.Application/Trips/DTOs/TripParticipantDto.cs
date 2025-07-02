namespace CarPooling.Application.Trips.DTOs
{
    public class TripParticipantDto
    {
        public int TripId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int SeatCount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime JoinedAt { get; set; }
    }
}
