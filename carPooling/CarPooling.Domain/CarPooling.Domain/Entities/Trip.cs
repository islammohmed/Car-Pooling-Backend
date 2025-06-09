using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using CarPooling.Domain.Enums;

namespace CarPooling.Domain.Entities
{

    public class Trip
    {
        [Key]
        public int TripId { get; set; }
        [Required]
        public string DriverId { get; set; }
        [ForeignKey(nameof(DriverId))]
        public User Driver { get; set; }
        [Required, Range(0, 10000)]
        public decimal PricePerSeat { get; set; }
        public TimeSpan EstimatedDuration { get; set; }
        [Required, Range(1, 10)]
        public int AvailableSeats { get; set; }
        [StringLength(500)]
        public string Notes { get; set; }
        [Required]
        public TripStatus Status { get; set; }
        [Required, StringLength(100)]
        public string SourceLocation { get; set; }
        [Required, StringLength(100)]
        public string Destination { get; set; }
        [Required]
        public DateTime StartTime { get; set; }
        [StringLength(500)]
        public string TripDescription { get; set; }
        public DateTime Created_At { get; set; }
        [StringLength(10)]
        public string GenderRequired { get; set; }

        public ICollection<TripParticipant> Participants { get; set; }

    }

}
