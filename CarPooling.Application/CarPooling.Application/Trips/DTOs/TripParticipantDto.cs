namespace CarPooling.Application.Trips.DTOs
{
    public class TripParticipantDto
    {
        public int TripId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int SeatCount { get; set; }
<<<<<<< HEAD
        public string Status { get; set; } = string.Empty;
        public DateTime JoinedAt { get; set; }
=======
        public string Status { get; set; }
        public DateTime JoinedAt { get; set; }= DateTime.Now;
>>>>>>> cfe46bee6df57510064209057ce19d1068710181
    }
}
