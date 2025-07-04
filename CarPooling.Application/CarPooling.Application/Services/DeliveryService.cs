using CarPooling.Application.DTOs;
using CarPooling.Application.Interfaces;
using CarPooling.Application.Interfaces.Repositories;
using CarPooling.Domain.Entities;
using CarPooling.Domain.Enums;
using CarPooling.Application.Trips.DTOs;
using CarPooling.Domain.DTOs;

namespace CarPooling.Application.Services
{
    public class DeliveryService : IDeliveryService
    {
        private readonly IDeliveryRequestRepository _deliveryRepository;
        private readonly IUserRepository _userRepository;
        private readonly ITripRepository _tripRepository;

        public DeliveryService(
            IDeliveryRequestRepository deliveryRepository,
            IUserRepository userRepository,
            ITripRepository tripRepository)
        {
            _deliveryRepository = deliveryRepository;
            _userRepository = userRepository;
            _tripRepository = tripRepository;
        }

        public async Task<ApiResponse<DeliveryRequestResponseDto>> CreateRequestAsync(string userId, CreateDeliveryRequestDto requestDto)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return ApiResponse<DeliveryRequestResponseDto>.ErrorResponse("User not found");
                }

                var deliveryRequest = new DeliveryRequest
                {
                    SenderId = userId,
                    ReceiverPhone = requestDto.ReceiverPhone,
                    DropoffLocation = requestDto.DropoffLocation,
                    SourceLocation = requestDto.SourceLocation,
                    Weight = requestDto.Weight,
                    ItemDescription = requestDto.ItemDescription,
                    Price = requestDto.Price,
                    Status = DeliveryStatus.Pending.ToString()
                };

                var created = await _deliveryRepository.CreateAsync(deliveryRequest);
                return ApiResponse<DeliveryRequestResponseDto>.SuccessResponse(MapToResponseDto(created, user.FirstName + " " + user.LastName));
            }
            catch (Exception ex)
            {
                return ApiResponse<DeliveryRequestResponseDto>.ErrorResponse($"Error creating delivery request: {ex.Message}");
            }
        }

        public async Task<ApiResponse<DeliveryRequestResponseDto>> AcceptDeliveryAsync(string driverId, int requestId, int tripId)
        {
            try
            {
                var driver = await _userRepository.GetByIdAsync(driverId);
                if (driver == null || driver.UserRole != UserRole.Driver)
                {
                    return ApiResponse<DeliveryRequestResponseDto>.ErrorResponse("Invalid driver");
                }

                var trip = await _tripRepository.GetByIdAsync(tripId);
                if (trip == null || trip.DriverId != driverId)
                {
                    return ApiResponse<DeliveryRequestResponseDto>.ErrorResponse("Invalid trip");
                }

                if (!trip.AcceptsDeliveries)
                {
                    return ApiResponse<DeliveryRequestResponseDto>.ErrorResponse("This trip does not accept deliveries");
                }

                var request = await _deliveryRepository.GetByIdAsync(requestId);
                if (request == null)
                {
                    return ApiResponse<DeliveryRequestResponseDto>.ErrorResponse("Delivery request not found");
                }

                if (request.Status != DeliveryStatus.Pending.ToString())
                {
                    return ApiResponse<DeliveryRequestResponseDto>.ErrorResponse("Request is no longer pending");
                }

                if (!IsLocationMatch(trip.SourceLocation, request.SourceLocation) || 
                    !IsLocationMatch(trip.Destination, request.DropoffLocation))
                {
                    return ApiResponse<DeliveryRequestResponseDto>.ErrorResponse(
                        "Delivery source and destination must match the trip's route");
                }

                if (trip.MaxDeliveryWeight.HasValue && request.Weight > trip.MaxDeliveryWeight.Value)
                {
                    return ApiResponse<DeliveryRequestResponseDto>.ErrorResponse(
                        $"Package weight exceeds trip's maximum delivery weight of {trip.MaxDeliveryWeight.Value}kg");
                }

                if (trip.StartTime < DateTime.UtcNow)
                {
                    return ApiResponse<DeliveryRequestResponseDto>.ErrorResponse("Cannot accept delivery for a past trip");
                }

                if (trip.Status != TripStatus.Pending && trip.Status != TripStatus.Confirmed)
                {
                    return ApiResponse<DeliveryRequestResponseDto>.ErrorResponse(
                        "Can only accept deliveries for pending or confirmed trips");
                }

                request.Status = DeliveryStatus.Accepted.ToString();
                request.TripId = tripId;

                var updated = await _deliveryRepository.UpdateAsync(request);
                return ApiResponse<DeliveryRequestResponseDto>.SuccessResponse(
                    MapToResponseDto(updated, request.Sender.FirstName + " " + request.Sender.LastName, 
                        driver.FirstName + " " + driver.LastName));
            }
            catch (Exception ex)
            {
                return ApiResponse<DeliveryRequestResponseDto>.ErrorResponse($"Error accepting delivery: {ex.Message}");
            }
        }

        public async Task<ApiResponse<DeliveryRequestResponseDto>> UpdateDeliveryStatusAsync(string driverId, int requestId, UpdateDeliveryStatusDto updateDto)
        {
            try
            {
                var request = await _deliveryRepository.GetByIdAsync(requestId);
                if (request == null)
                {
                    return ApiResponse<DeliveryRequestResponseDto>.ErrorResponse("Delivery request not found");
                }

                if (!request.TripId.HasValue)
                {
                    return ApiResponse<DeliveryRequestResponseDto>.ErrorResponse("This delivery has not been accepted by any driver");
                }

                var trip = await _tripRepository.GetByIdAsync(request.TripId.Value);
                if (trip == null || trip.DriverId != driverId)
                {
                    return ApiResponse<DeliveryRequestResponseDto>.ErrorResponse("Unauthorized to update this delivery");
                }

                if (!Enum.TryParse<DeliveryStatus>(updateDto.Status, out var newStatus))
                {
                    return ApiResponse<DeliveryRequestResponseDto>.ErrorResponse("Invalid status");
                }

                var currentStatus = Enum.Parse<DeliveryStatus>(request.Status);
                if (!IsValidStatusTransition(currentStatus, newStatus))
                {
                    return ApiResponse<DeliveryRequestResponseDto>.ErrorResponse("Invalid status transition");
                }

                request.Status = newStatus.ToString();
                var updated = await _deliveryRepository.UpdateAsync(request);
                
                var driver = await _userRepository.GetByIdAsync(driverId);
                return ApiResponse<DeliveryRequestResponseDto>.SuccessResponse(
                    MapToResponseDto(updated, request.Sender.FirstName + " " + request.Sender.LastName, driver?.FirstName + " " + driver?.LastName));
            }
            catch (Exception ex)
            {
                return ApiResponse<DeliveryRequestResponseDto>.ErrorResponse($"Error updating delivery status: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> CancelRequestAsync(string userId, int requestId)
        {
            try
            {
                var request = await _deliveryRepository.GetByIdAsync(requestId);
                if (request == null)
                {
                    return ApiResponse<bool>.ErrorResponse("Delivery request not found");
                }

                if (request.SenderId != userId)
                {
                    return ApiResponse<bool>.ErrorResponse("Unauthorized to cancel this request");
                }

                if (request.Status != DeliveryStatus.Pending.ToString())
                {
                    return ApiResponse<bool>.ErrorResponse("Can only cancel pending requests");
                }

                request.Status = DeliveryStatus.Cancelled.ToString();
                await _deliveryRepository.UpdateAsync(request);
                return ApiResponse<bool>.SuccessResponse(true);
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResponse($"Error cancelling request: {ex.Message}");
            }
        }

        public async Task<ApiResponse<DeliveryRequestResponseDto>> GetRequestByIdAsync(int id)
        {
            try
            {
                var request = await _deliveryRepository.GetByIdAsync(id);
                if (request == null)
                {
                    return ApiResponse<DeliveryRequestResponseDto>.ErrorResponse("Delivery request not found");
                }

                var sender = await _userRepository.GetByIdAsync(request.SenderId);
                string? driverName = null;

                if (request.TripId.HasValue)
                {
                    var trip = await _tripRepository.GetByIdAsync(request.TripId.Value);
                    if (trip != null)
                    {
                        var driver = await _userRepository.GetByIdAsync(trip.DriverId);
                        if (driver != null)
                        {
                            driverName = $"{driver.FirstName} {driver.LastName}";
                        }
                    }
                }

                return ApiResponse<DeliveryRequestResponseDto>.SuccessResponse(
                    MapToResponseDto(request, sender?.FirstName + " " + sender?.LastName, driverName));
            }
            catch (Exception ex)
            {
                return ApiResponse<DeliveryRequestResponseDto>.ErrorResponse($"Error retrieving delivery request: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<DeliveryRequestResponseDto>>> GetPendingRequestsAsync()
        {
            try
            {
                var requests = await _deliveryRepository.GetPendingRequestsAsync();
                var responseDtos = new List<DeliveryRequestResponseDto>();

                foreach (var request in requests)
                {
                    var sender = await _userRepository.GetByIdAsync(request.SenderId);
                    responseDtos.Add(MapToResponseDto(request, sender?.FirstName + " " + sender?.LastName));
                }

                return ApiResponse<List<DeliveryRequestResponseDto>>.SuccessResponse(responseDtos);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<DeliveryRequestResponseDto>>.ErrorResponse($"Error retrieving pending requests: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<DeliveryRequestResponseDto>>> GetUserRequestsAsync(string userId)
        {
            try
            {
                var requests = await _deliveryRepository.GetUserRequestsAsync(userId);
                var responseDtos = new List<DeliveryRequestResponseDto>();

                foreach (var request in requests)
                {
                    var sender = await _userRepository.GetByIdAsync(request.SenderId);
                    string? driverName = null;

                    if (request.TripId.HasValue)
                    {
                        var trip = await _tripRepository.GetByIdAsync(request.TripId.Value);
                        if (trip != null)
                        {
                            var driver = await _userRepository.GetByIdAsync(trip.DriverId);
                            if (driver != null)
                            {
                                driverName = $"{driver.FirstName} {driver.LastName}";
                            }
                        }
                    }

                    responseDtos.Add(MapToResponseDto(request, sender?.FirstName + " " + sender?.LastName, driverName));
                }

                return ApiResponse<List<DeliveryRequestResponseDto>>.SuccessResponse(responseDtos);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<DeliveryRequestResponseDto>>.ErrorResponse($"Error retrieving user requests: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<DeliveryRequestResponseDto>>> GetDriverDeliveriesAsync(string driverId)
        {
            try
            {
                var deliveries = await _deliveryRepository.GetDriverDeliveriesAsync(driverId);
                var responseDtos = new List<DeliveryRequestResponseDto>();

                foreach (var delivery in deliveries)
                {
                    var sender = await _userRepository.GetByIdAsync(delivery.SenderId);
                    var driver = await _userRepository.GetByIdAsync(driverId);
                    responseDtos.Add(MapToResponseDto(delivery, sender?.FirstName + " " + sender?.LastName, driver?.FirstName + " " + driver?.LastName));
                }

                return ApiResponse<List<DeliveryRequestResponseDto>>.SuccessResponse(responseDtos);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<DeliveryRequestResponseDto>>.ErrorResponse($"Error retrieving driver deliveries: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<TripListDto>>> GetMatchingTripsAsync(int requestId)
        {
            try
            {
                var request = await _deliveryRepository.GetByIdAsync(requestId);
                if (request == null)
                {
                    return ApiResponse<List<TripListDto>>.ErrorResponse("Delivery request not found");
                }

                // Get all trips that:
                // 1. Accept deliveries
                // 2. Have matching source/destination
                // 3. Have sufficient weight capacity
                // 4. Are in Pending or Confirmed status
                // 5. Haven't started yet
                var allTrips = await _tripRepository.GetAllTripsAsync(new PaginationParams { PageSize = 100, PageNumber = 1 });
                var matchingTrips = allTrips.Items
                    .Where(t => t.AcceptsDeliveries &&
                               IsLocationMatch(t.SourceLocation, request.SourceLocation) &&
                               IsLocationMatch(t.Destination, request.DropoffLocation) &&
                               (!t.MaxDeliveryWeight.HasValue || t.MaxDeliveryWeight.Value >= request.Weight) &&
                               (t.Status == TripStatus.Pending || t.Status == TripStatus.Confirmed) &&
                               t.StartTime > DateTime.UtcNow)
                    .ToList();

                var tripDtos = matchingTrips.Select(trip => new TripListDto
                {
                    TripId = trip.TripId,
                    DriverName = $"{trip.Driver.FirstName} {trip.Driver.LastName}",
                    PricePerSeat = trip.PricePerSeat,
                    EstimatedDuration = trip.EstimatedDuration,
                    AvailableSeats = trip.AvailableSeats,
                    TripDescription = trip.TripDescription,
                    Status = trip.Status,
                    SourceLocation = trip.SourceLocation,
                    Destination = trip.Destination,
                    StartTime = trip.StartTime,
                    CreatedAt = trip.CreatedAt,
                    GenderPreference = trip.GenderPreference,
                    ParticipantsCount = trip.Participants.Count
                }).ToList();

                return ApiResponse<List<TripListDto>>.SuccessResponse(tripDtos);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<TripListDto>>.ErrorResponse($"Error finding matching trips: {ex.Message}");
            }
        }

        private DeliveryRequestResponseDto MapToResponseDto(DeliveryRequest request, string? senderName, string? driverName = null)
        {
            return new DeliveryRequestResponseDto
            {
                Id = request.Id,
                ReceiverPhone = request.ReceiverPhone,
                DropoffLocation = request.DropoffLocation,
                SourceLocation = request.SourceLocation,
                Weight = request.Weight,
                ItemDescription = request.ItemDescription,
                Price = request.Price,
                Status = request.Status,
                SenderId = request.SenderId,
                SenderName = senderName ?? "Unknown",
                TripId = request.TripId,
                DriverName = driverName,
                EstimatedDeliveryTime = request.Trip?.StartTime
            };
        }

        private bool IsValidStatusTransition(DeliveryStatus currentStatus, DeliveryStatus newStatus)
        {
            return (currentStatus, newStatus) switch
            {
                (DeliveryStatus.Pending, DeliveryStatus.Accepted) => true,
                (DeliveryStatus.Pending, DeliveryStatus.Rejected) => true,
                (DeliveryStatus.Accepted, DeliveryStatus.InTransit) => true,
                (DeliveryStatus.InTransit, DeliveryStatus.Delivered) => true,
                _ => false
            };
        }

        private bool IsLocationMatch(string location1, string location2)
        {
            return location1.Trim().Equals(location2.Trim(), StringComparison.OrdinalIgnoreCase);
        }
    }
} 