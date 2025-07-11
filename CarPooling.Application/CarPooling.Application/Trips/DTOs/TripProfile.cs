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
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.User.FirstName} {src.User.LastName}"))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.User.PhoneNumber))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
                .ForMember(dest => dest.Rating, opt => opt.MapFrom(src => src.User.AvgRating))
                .ForMember(dest => dest.ProfileImage, opt => opt.MapFrom(src => src.User.NationalIdImage))
                .ForMember(dest => dest.UserRole, opt => opt.MapFrom(src => src.User.UserRole))
                .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.User.Gender))
                .ForMember(dest => dest.IsDriver, opt => opt.MapFrom(src => false));
                
            CreateMap<Trip, TripListDto>()
                .ForMember(dest => dest.DriverId, 
                    opt => opt.MapFrom(src => src.DriverId))
                .ForMember(dest => dest.DriverName, 
                    opt => opt.MapFrom(src => $"{src.Driver.FirstName} {src.Driver.LastName}"))
                .ForMember(dest => dest.ParticipantsCount, 
                    opt => opt.MapFrom(src => src.Participants.Count))
                .ForMember(dest => dest.CreatedAt,
                    opt => opt.MapFrom(src => src.CreatedAt));
        }
    }
}
