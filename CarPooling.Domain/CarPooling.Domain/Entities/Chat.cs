using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;


namespace CarPooling.Domain.Entities
{
    public class Chat
    {
        public int Id { get; set; }
        public string Message { get; set; }
        public DateTime SentAt { get; set; }
        public bool IsRead { get; set; }

        public int TripId { get; set; }
        public string SenderId { get; set; }
        public string ReceiverId { get; set; }

        public Trip Trip { get; set; }
        public User Sender { get; set; }
        public User Receiver { get; set; }
    }
}
