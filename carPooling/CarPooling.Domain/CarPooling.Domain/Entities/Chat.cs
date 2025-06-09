using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarPooling.Domain.Entities
{
    public class Chat
    {
        [Key]
        public int Chat_ID { get; set; }

        [ForeignKey("Trip")]
        public int Trip_ID { get; set; }

        [ForeignKey("Sender")]
        public string Sender_ID { get; set; }

        [ForeignKey("Receiver")]
        public string Receiver_ID { get; set; }

        [Required]
        public string Message { get; set; }

        public DateTime Sent_At { get; set; }

        public bool IsRead { get; set; }

        public Trip Trip { get; set; }
        public User Sender { get; set; }
        public User Receiver { get; set; }
    }
}
