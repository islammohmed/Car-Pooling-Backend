
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;


namespace CarPooling.Domain.Entities
{
    public class DeliveryRequest
    {
        [Key]
        public int Id { get; set; }
        [ Phone]
        public string ReceiverPhone { get; set; }
        public string DropoffLocation { get; set; }
        public string SourceLocation { get; set; }
        public float Weight { get; set; }
        public string ItemDescription { get; set; }
        public decimal Price { get; set; }
        public string Status { get; set; }

        [ForeignKey("Sender")]
        public string SenderId { get; set; }
        public int TripId { get; set; }
        public User Sender { get; set; }
        public Trip Trip { get; set; }
    }

}
