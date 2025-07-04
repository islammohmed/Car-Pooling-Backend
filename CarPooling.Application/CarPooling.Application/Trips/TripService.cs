using AutoMapper;
using CarPooling.Application.Interfaces;
using CarPooling.Application.Trips.DTOs;
using CarPooling.Application.DTOs;
using CarPooling.Domain.Entities;
using FluentValidation;
using System;

namespace CarPooling.Application.Trips
{
    public class TripService : ITripService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IValidator<CreateTripDto> _validator;

        public TripService(
            IUnitOfWork unitOfWork, 
            IMapper mapper,
            IValidator<CreateTripDto> validator)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _validator = validator;
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

        public async Task<TripListDto?> GetTripByIdAsync(int tripId)
        {
            var trip = await _unitOfWork.Trips.GetByIdAsync(tripId);
            return trip == null ? null : _mapper.Map<TripListDto>(trip);
        }
        
        public async Task<int> CreateTripAsync(CreateTripDto tripDto)
        {
            // Validate the trip data
            var validationResult = await _validator.ValidateAsync(tripDto);
            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                throw new ValidationException($"Invalid trip creation request: {errors}");
            }
            
            // Map DTO to entity
            var trip = _mapper.Map<Trip>(tripDto);
            
            // Set CreatedAt to current UTC time
            trip.CreatedAt = DateTime.UtcNow;
            
            // Save to database
            var tripId = await _unitOfWork.Trips.CreateAsync(trip);
            await _unitOfWork.SaveChangesAsync();
            
            return tripId;
        }
    }
} 