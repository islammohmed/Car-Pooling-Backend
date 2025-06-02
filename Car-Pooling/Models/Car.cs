using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Car_Pooling.Models
{
    public class Car
    {
        [Key]
        public int CarId { get; set; }
        [Required, StringLength (50)]
        public string Model { get; set; }
        [Required, StringLength (20)]
        public string Color { get; set; }
        [Required]
        public string DriverId { get; set; }
        [ForeignKey (nameof (DriverId))]
        public User Driver { get; set; }

        [Required, StringLength (20)]
        public string PlateNumber { get; set; }
    }
}
