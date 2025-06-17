
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;


namespace CarPooling.Domain.Entities
{
    public class DeliveryRequest
    {
        [Key]
        public int Request_ID { get; set; }

        [ForeignKey("Sender")]
        public string Sender_ID { get; set; }

        [Required, Phone]
        public string Receiver_Phone { get; set; }

        [Required]
        public string Dropoff_Location { get; set; }

        [Required]
        public string Source_Location { get; set; }

        public float Weight { get; set; }

        [Required]
        public string Item_Description { get; set; }

        [Required]
        public decimal price { get; set; }

        public string Status { get; set; }

        [ForeignKey("AssignedTrip")]
        public int? Assigned_Trip_ID { get; set; }

        public User Sender { get; set; }
        public Trip AssignedTrip { get; set; }
    }

}
