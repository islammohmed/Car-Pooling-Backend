namespace CarPooling.Domain.Entities
{
    public class Chat
    {
        public int Id { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
        public bool IsRead { get; set; }

        public int TripId { get; set; }
        public string SenderId { get; set; } = string.Empty;
        public string ReceiverId { get; set; } = string.Empty;

        public Trip Trip { get; set; } = null!;
        public User Sender { get; set; } = null!;
        public User Receiver { get; set; } = null!;
    }
}
