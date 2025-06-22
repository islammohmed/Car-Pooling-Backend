
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;


namespace CarPooling.Domain.Entities
{
    public class Payment
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public DateTime TransactionDate { get; set; }
        public string TransactionRef { get; set; }
        public string PaymentType { get; set; }
        public string PaymentStatus { get; set; }


        public int TripId { get; set; }

        public string PayerId { get; set; }
        public string ReceiveId { get; set; }
        public Trip Trip { get; set; }
        public User Payer { get; set; }
        public User Receiver { get; set; }
    }

}
