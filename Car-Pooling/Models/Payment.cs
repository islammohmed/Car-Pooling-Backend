using Car_Pooling.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Payment
{
    [Key]
    public int ID { get; set; }

    [ForeignKey("Trip")]
    public int Trip_ID { get; set; }

    [Required]
    public decimal Amount { get; set; }

    public DateTime Transaction_Date { get; set; }

    public string Transaction_Ref { get; set; }

    [ForeignKey("Payer")]
    public int Payer_ID { get; set; }

    [ForeignKey("Receiver")]
    public int Receiver_User_ID { get; set; }

    public string Payment_Type { get; set; }

    public string Payment_Status { get; set; }

    public Trip Trip { get; set; }
    public User Payer { get; set; }
    public User Receiver { get; set; }
}
