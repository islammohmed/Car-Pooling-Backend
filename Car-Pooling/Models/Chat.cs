using Car_Pooling.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Chat
{
    [Key]
    public int Chat_ID { get; set; }

    [ForeignKey("Trip")]
    public int Trip_ID { get; set; }

    [ForeignKey("Sender")]
    public int Sender_ID { get; set; }

    [ForeignKey("Receiver")]
    public int Receiver_ID { get; set; }

    [Required]
    public string Message { get; set; }

    public DateTime Sent_At { get; set; }

    public bool IsRead { get; set; }

    public Trip Trip { get; set; }
    public User Sender { get; set; }
    public User Receiver { get; set; }
}
