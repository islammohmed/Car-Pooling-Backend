using Car_Pooling.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Feedback
{
    [Key]
    public int Feedback_ID { get; set; }

    [ForeignKey("Trip")]
    public int Trip_ID { get; set; }

    public string Comment { get; set; }

    [Range(1, 5)]
    public int Rating { get; set; }

    [ForeignKey("Receiver")]
    public int Receiver_User_ID { get; set; }

    [ForeignKey("Sender")]
    public int Sender_User_ID { get; set; }

    public Trip Trip { get; set; }
    public User Sender { get; set; }
    public User Receiver { get; set; }
}
