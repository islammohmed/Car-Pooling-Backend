
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using CarPooling.Domain.Enums;

namespace CarPooling.Domain.Entities
{

    public class Trip
    {
        public int Id { get; set; }
        [ Range(0, 10000)]
        public decimal PricePerSeat { get; set; }
        public TimeSpan EstimatedDuration { get; set; }
        [ Range(1, 50)]
        public int AvailableSeats { get; set; }
        public string TripDescription { get; set; }
        [StringLength(500)]
        public string Notes { get; set; }
        public TripStatus Status { get; set; }
        [ StringLength(100)]
        public string SourceLocation { get; set; }
        [ StringLength(100)]
        public string Destination { get; set; }
        [Required]
        public DateTime StartTime { get; set; }
        [StringLength(500)]
        public DateTime Created_At { get; set; }
        [StringLength(10)]
        public Gender?  GenderPreference { get; set; }

        public string DriverId { get; set; }
        [ForeignKey(nameof(DriverId))]
        public User Driver { get; set; }
        public ICollection<TripParticipant> Participants { get; set; }

    }

}
