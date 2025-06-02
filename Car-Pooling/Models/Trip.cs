using Car_Pooling.Data.Enums;
using System.ComponentModel.DataAnnotations;
using System.Net.NetworkInformation;

namespace Car_Pooling.Models
{
    public class Trip
    {
        [Key]
        public int TripID { get; set; }
        [Required]
        public int DriverID { get; set; }

        public int AvailableSeats { get; set; }
        public string Notes { get; set; }
        public decimal pricePerSeat { get; set; }
        public TripStatus Status { get; set; }
        public string Source_Location { get; set; }
        public string Destination { get; set; }
        public TimeSpan Estimated_Duration { get; set; }
        public DateTime Start_Time { get; set; }
        public string Trip_Description { get; set; }
        public DateTime Created_At { get; set; }
        public string GenderRequired { get; set; }
        public bool Is_Confirmed { get; set; }
        public bool Is_Completed { get; set; }
    }
}
