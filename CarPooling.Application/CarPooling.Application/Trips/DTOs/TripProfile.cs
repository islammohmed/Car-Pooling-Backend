using CarPooling.Application.Trips.Commands.CreateRequest;
using AutoMapper;
using CarPooling.Domain.Entities;


namespace CarPooling.Application.Trips.DTOs
{
    public class TripProfile : Profile
    {
        public TripProfile()
        {
            CreateMap<CreateTripCommand, Trip>();
        }
    }
}
