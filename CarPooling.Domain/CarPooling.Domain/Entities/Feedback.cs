using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;


namespace CarPooling.Domain.Entities
{
    public class Feedback
    {
        public int  Id { get; set; }
        public string Comment { get; set; } = string.Empty;
        [Range(1, 5)]
        public int Rating { get; set; }

        public int TripId { get; set; }
        public string ReceiverId { get; set; } = string.Empty;
        public string SenderId { get; set; } = string.Empty;
        public Trip Trip { get; set; } = null!;
        public User Sender { get; set; } = null!;
        public User Receiver { get; set; } = null!;
    }

}
