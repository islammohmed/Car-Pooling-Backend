using AutoMapper;
using CarPooling.Application.Interfaces;
using CarPooling.Application.Trips.DTOs;

namespace CarPooling.Application.Trips
{
    public class TripService : ITripService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public TripService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
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
    }
} 