using CarPooling.Domain.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarPooling.Application.Trips.Commands.CreateRequest
{
    public class CreateTripCommand:IRequest<int>
    {
        public string DriverId { get; set; }
        public decimal PricePerSeat { get; set; }
        public TimeSpan EstimatedDuration { get; set; }
        public int AvailableSeats { get; set; }
        public string Notes { get; set; }
        public TripStatus Status { get; set; }
        public string SourceLocation { get; set; }
        public string Destination { get; set; }
        public DateTime StartTime { get; set; }
        public string TripDescription { get; set; }
        public Gender GenderPreference { get; set; }
    }
}
