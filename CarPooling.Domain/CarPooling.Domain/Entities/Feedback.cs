
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;


namespace CarPooling.Domain.Entities
{
    public class Feedback
    {
        public int  Id { get; set; }
        public string Comment { get; set; }
        [Range(1, 5)]
        public int Rating { get; set; }

        public int TripId { get; set; }
        public string ReceiverId { get; set; }
        public string SenderId { get; set; }
        public Trip Trip { get; set; }
        public User Sender { get; set; }
        public User Receiver { get; set; }
    }

}
