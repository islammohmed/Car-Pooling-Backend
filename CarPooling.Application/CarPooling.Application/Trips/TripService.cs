using AutoMapper;
using CarPooling.Application.Interfaces;
using CarPooling.Application.Trips.DTOs;
using CarPooling.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace CarPooling.Application.Trips
{
    public class TripService : ITripService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<TripService> _logger;

        public TripService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<TripService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<PaginatedResponse<TripListDto>> GetAllTripsAsync(PaginationParams paginationParams)
        {
            var (trips, totalCount) = await _unitOfWork.Trips.GetAllTripsAsync(paginationParams);
            
            var tripDtos = _mapper.Map<IEnumerable<TripListDto>>(trips);

            return new PaginatedResponse<TripListDto>
            {
                Items = tripDtos,
                PageNumber = paginationParams.PageNumber,
                PageSize = paginationParams.PageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)paginationParams.PageSize)
            };
        }

        public async Task<int> CreateTripAsync(CreateTripDto tripDto)
        {
            _logger.LogInformation("Creating a new trip");
            var trip = _mapper.Map<Trip>(tripDto);

            await _unitOfWork.Trips.CreateAsync(trip);
            await _unitOfWork.SaveChangesAsync();

            return trip.TripId;
        }
    }
} 