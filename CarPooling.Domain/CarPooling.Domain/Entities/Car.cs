using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;


namespace CarPooling.Domain.Entities
{
    public class Car
    {
        public int Id { get; set; }
        [ StringLength(50)]
        public string Model { get; set; } = string.Empty;
        [StringLength(20)]
        public string Color { get; set; } = string.Empty;
        [StringLength(20)]
        public string PlateNumber { get; set; } = string.Empty;
        public string CarLicenseImage { get; set; } = string.Empty;
        public bool IsVerified { get; set; }
        public string DriverId { get; set; } = string.Empty;
        [ForeignKey(nameof(DriverId))]
        public User Driver { get; set; } = null!;

    }
}
