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
            CreateMap<TripParticipant, TripParticipantDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
            CreateMap<Trip, TripListDto>()
                .ForMember(dest => dest.DriverName, 
                    opt => opt.MapFrom(src => src.Driver.UserName))
                .ForMember(dest => dest.ParticipantsCount, 
                    opt => opt.MapFrom(src => src.Participants.Count))
                .ForMember(dest => dest.CreatedAt,
                    opt => opt.MapFrom(src => src.CreatedAt));
        }
    }
}
