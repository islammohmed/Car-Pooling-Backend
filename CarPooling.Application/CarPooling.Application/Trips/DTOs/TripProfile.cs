using AutoMapper;
using CarPooling.Domain.Entities;
using CarPooling.Application.DTOs;

namespace CarPooling.Application.Trips.DTOs
{
    public class TripProfile : Profile
    {
        public TripProfile()
        {
            CreateMap<CreateTripDto, Trip>();
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
