using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;


namespace CarPooling.Domain.Entities
{
    public class Payment
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public DateTime TransactionDate { get; set; }
        public string TransactionRef { get; set; } = string.Empty;
        public string PaymentType { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;


        public int TripId { get; set; }

        public string PayerId { get; set; } = string.Empty;
        public string ReceiveId { get; set; } = string.Empty;
        public Trip Trip { get; set; } = null!;
        public User Payer { get; set; } = null!;
        public User Receiver { get; set; } = null!;
    }

}
