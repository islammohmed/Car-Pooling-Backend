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
                    Status = DeliveryStatus.Pending.ToString(),
                    DeliveryStartDate = requestDto.DeliveryStartDate,
                    DeliveryEndDate = requestDto.DeliveryEndDate,
                    CreatedAt = DateTime.UtcNow
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

                // Accept both Pending and TripSelected statuses
                if (request.Status != DeliveryStatus.Pending.ToString() && request.Status != DeliveryStatus.TripSelected.ToString())
                {
                    return ApiResponse<DeliveryRequestResponseDto>.ErrorResponse("Request is not in a state that can be accepted");
                }
                
                // For TripSelected status, verify the trip matches the one selected by the user
                if (request.Status == DeliveryStatus.TripSelected.ToString() && request.TripId != tripId)
                {
                    return ApiResponse<DeliveryRequestResponseDto>.ErrorResponse("This delivery request was selected for a different trip");
                }
                
                // Check if request is expired
                if (request.DeliveryEndDate < DateTime.UtcNow)
                {
                    // Automatically update the status to cancelled
                    request.Status = DeliveryStatus.Cancelled.ToString();
                    await _deliveryRepository.UpdateAsync(request);
                    return ApiResponse<DeliveryRequestResponseDto>.ErrorResponse("This delivery request has expired and has been cancelled");
                }

                // Only verify location match if the status is Pending (not needed for TripSelected as it was already verified)
                if (request.Status == DeliveryStatus.Pending.ToString())
                {
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
                request.AcceptedAt = DateTime.UtcNow;

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

                // Update the status
                request.Status = newStatus.ToString();
                
                // Update timestamps based on the new status
                switch (newStatus)
                {
                    case DeliveryStatus.Accepted:
                        request.AcceptedAt = DateTime.UtcNow;
                        break;
                    case DeliveryStatus.InTransit:
                        request.PickedUpAt = DateTime.UtcNow;
                        break;
                    case DeliveryStatus.Delivered:
                        request.DeliveredAt = DateTime.UtcNow;
                        break;
                }
                
                // Update delivery notes if provided
                if (!string.IsNullOrEmpty(updateDto.Notes))
                {
                    request.DeliveryNotes = updateDto.Notes;
                }

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

                if (request.Status != DeliveryStatus.Pending.ToString() && request.Status != DeliveryStatus.TripSelected.ToString())
                {
                    return ApiResponse<bool>.ErrorResponse("Can only cancel pending or trip-selected requests");
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
                    // Check if request is expired
                    if (request.DeliveryEndDate < DateTime.UtcNow)
                    {
                        // Update status to cancelled for expired requests
                        request.Status = DeliveryStatus.Cancelled.ToString();
                        await _deliveryRepository.UpdateAsync(request);
                        // Skip adding this to the response
                        continue;
                    }
                    
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
                    // Check if pending request is expired
                    if (request.Status == DeliveryStatus.Pending.ToString() && request.DeliveryEndDate < DateTime.UtcNow)
                    {
                        // Update status to cancelled for expired requests
                        request.Status = DeliveryStatus.Cancelled.ToString();
                        await _deliveryRepository.UpdateAsync(request);
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
                
                // Only check for expiration if the request is still in Pending status
                if (request.Status == DeliveryStatus.Pending.ToString() && request.DeliveryEndDate < DateTime.UtcNow)
                {
                    // Automatically update the status to cancelled
                    request.Status = DeliveryStatus.Cancelled.ToString();
                    await _deliveryRepository.UpdateAsync(request);
                    return ApiResponse<List<TripListDto>>.ErrorResponse("This delivery request has expired and has been cancelled");
                }

                // If request is not in Pending status, it can't match with trips
                if (request.Status != DeliveryStatus.Pending.ToString())
                {
                    return ApiResponse<List<TripListDto>>.ErrorResponse($"Cannot find matching trips for a request with status: {request.Status}");
                }

                // Get all trips that:
                // 1. Accept deliveries
                // 2. Have matching source/destination
                // 3. Have sufficient weight capacity
                // 4. Are in Pending or Confirmed status
                // 5. Haven't started yet
                // 6. Trip start time falls within the delivery date range
                var allTrips = await _tripRepository.GetAllTripsAsync(new PaginationParams { PageSize = 100, PageNumber = 1 });
                var matchingTrips = allTrips.Items
                    .Where(t => t.AcceptsDeliveries &&
                               IsLocationMatch(t.SourceLocation, request.SourceLocation) &&
                               IsLocationMatch(t.Destination, request.DropoffLocation) &&
                               (!t.MaxDeliveryWeight.HasValue || t.MaxDeliveryWeight.Value >= request.Weight) &&
                               (t.Status == TripStatus.Pending || t.Status == TripStatus.Confirmed) &&
                               t.StartTime > DateTime.UtcNow &&
                               t.StartTime >= request.DeliveryStartDate &&
                               t.StartTime <= request.DeliveryEndDate)
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

        public async Task<ApiResponse<DeliveryRequestResponseDto>> SelectTripForDeliveryAsync(string userId, int requestId, SelectTripForDeliveryDto selectTripDto)
        {
            try
            {
                var request = await _deliveryRepository.GetByIdAsync(requestId);
                if (request == null)
                {
                    return ApiResponse<DeliveryRequestResponseDto>.ErrorResponse("Delivery request not found");
                }

                if (request.SenderId != userId)
                {
                    return ApiResponse<DeliveryRequestResponseDto>.ErrorResponse("Unauthorized to select a trip for this delivery request");
                }

                if (request.Status != DeliveryStatus.Pending.ToString())
                {
                    return ApiResponse<DeliveryRequestResponseDto>.ErrorResponse("Can only select a trip for pending requests");
                }
                
                // Check if request is expired
                if (request.DeliveryEndDate < DateTime.UtcNow)
                {
                    // Automatically update the status to cancelled
                    request.Status = DeliveryStatus.Cancelled.ToString();
                    await _deliveryRepository.UpdateAsync(request);
                    return ApiResponse<DeliveryRequestResponseDto>.ErrorResponse("This delivery request has expired and has been cancelled");
                }

                // Verify the trip exists and can accept deliveries
                var trip = await _tripRepository.GetByIdAsync(selectTripDto.TripId);
                if (trip == null)
                {
                    return ApiResponse<DeliveryRequestResponseDto>.ErrorResponse("Trip not found");
                }

                if (!trip.AcceptsDeliveries)
                {
                    return ApiResponse<DeliveryRequestResponseDto>.ErrorResponse("This trip does not accept deliveries");
                }

                if (trip.StartTime < DateTime.UtcNow)
                {
                    return ApiResponse<DeliveryRequestResponseDto>.ErrorResponse("Cannot select a past trip");
                }

                if (trip.Status != TripStatus.Pending && trip.Status != TripStatus.Confirmed)
                {
                    return ApiResponse<DeliveryRequestResponseDto>.ErrorResponse(
                        "Can only select pending or confirmed trips");
                }

                // Verify locations match
                if (!IsLocationMatch(trip.SourceLocation, request.SourceLocation) || 
                    !IsLocationMatch(trip.Destination, request.DropoffLocation))
                {
                    return ApiResponse<DeliveryRequestResponseDto>.ErrorResponse(
                        "Delivery source and destination must match the trip's route");
                }

                // Verify weight is acceptable
                if (trip.MaxDeliveryWeight.HasValue && request.Weight > trip.MaxDeliveryWeight.Value)
                {
                    return ApiResponse<DeliveryRequestResponseDto>.ErrorResponse(
                        $"Package weight exceeds trip's maximum delivery weight of {trip.MaxDeliveryWeight.Value}kg");
                }

                // Update the request with the selected trip
                request.TripId = selectTripDto.TripId;
                request.Status = DeliveryStatus.TripSelected.ToString();
                
                // Store delivery notes if provided
                if (!string.IsNullOrEmpty(selectTripDto.DeliveryNotes))
                {
                    request.DeliveryNotes = selectTripDto.DeliveryNotes;
                }

                var updated = await _deliveryRepository.UpdateAsync(request);
                
                // Get driver name for response
                var driver = await _userRepository.GetByIdAsync(trip.DriverId);
                var driverName = driver != null ? $"{driver.FirstName} {driver.LastName}" : null;
                
                return ApiResponse<DeliveryRequestResponseDto>.SuccessResponse(
                    MapToResponseDto(updated, request.Sender.FirstName + " " + request.Sender.LastName, driverName));
            }
            catch (Exception ex)
            {
                return ApiResponse<DeliveryRequestResponseDto>.ErrorResponse($"Error selecting trip for delivery: {ex.Message}");
            }
        }

        public async Task<int> HandleExpiredRequestsAsync()
        {
            try
            {
                // Get all pending requests
                var pendingRequests = await _deliveryRepository.GetPendingRequestsAsync();
                
                // Filter for expired requests (where the end date has passed)
                var expiredRequests = pendingRequests
                    .Where(r => (r.Status == DeliveryStatus.Pending.ToString() || 
                                r.Status == DeliveryStatus.TripSelected.ToString()) && 
                           r.DeliveryEndDate < DateTime.UtcNow)
                    .ToList();
                
                // Update status to cancelled for all expired requests
                foreach (var request in expiredRequests)
                {
                    request.Status = DeliveryStatus.Cancelled.ToString();
                    await _deliveryRepository.UpdateAsync(request);
                }
                
                return expiredRequests.Count;
            }
            catch (Exception)
            {
                // Log the error but don't throw, as this is a background operation
                return 0;
            }
        }

        public async Task<ApiResponse<DeliveryRequestResponseDto>> RejectDeliveryAsync(string driverId, int requestId)
        {
            try
            {
                var driver = await _userRepository.GetByIdAsync(driverId);
                if (driver == null || driver.UserRole != UserRole.Driver)
                {
                    return ApiResponse<DeliveryRequestResponseDto>.ErrorResponse("Invalid driver");
                }

                var request = await _deliveryRepository.GetByIdAsync(requestId);
                if (request == null)
                {
                    return ApiResponse<DeliveryRequestResponseDto>.ErrorResponse("Delivery request not found");
                }

                // Can only reject TripSelected requests that are associated with this driver's trip
                if (request.Status != DeliveryStatus.TripSelected.ToString())
                {
                    return ApiResponse<DeliveryRequestResponseDto>.ErrorResponse("Can only reject requests that have been selected for a trip");
                }

                if (!request.TripId.HasValue)
                {
                    return ApiResponse<DeliveryRequestResponseDto>.ErrorResponse("This request is not associated with any trip");
                }

                var trip = await _tripRepository.GetByIdAsync(request.TripId.Value);
                if (trip == null || trip.DriverId != driverId)
                {
                    return ApiResponse<DeliveryRequestResponseDto>.ErrorResponse("Unauthorized to reject this delivery request");
                }

                request.Status = DeliveryStatus.Rejected.ToString();
                // Clear the TripId to allow the user to select another trip
                request.TripId = null;

                var updated = await _deliveryRepository.UpdateAsync(request);
                return ApiResponse<DeliveryRequestResponseDto>.SuccessResponse(
                    MapToResponseDto(updated, request.Sender.FirstName + " " + request.Sender.LastName));
            }
            catch (Exception ex)
            {
                return ApiResponse<DeliveryRequestResponseDto>.ErrorResponse($"Error rejecting delivery: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<DeliveryRequestResponseDto>>> GetSelectedDeliveriesForDriverAsync(string driverId)
        {
            try
            {
                // Get all trips owned by this driver
                var driverTrips = await _tripRepository.GetDriverTripsAsync(driverId);
                
                if (driverTrips == null || !driverTrips.Any())
                {
                    return ApiResponse<List<DeliveryRequestResponseDto>>.SuccessResponse(new List<DeliveryRequestResponseDto>());
                }
                
                // Get all delivery requests that are in TripSelected status and associated with any of the driver's trips
                var tripIds = driverTrips.Select(t => t.TripId).ToList();
                var selectedDeliveries = await _deliveryRepository.GetDeliveriesByStatusAndTripsAsync(DeliveryStatus.TripSelected.ToString(), tripIds);
                
                var responseDtos = new List<DeliveryRequestResponseDto>();
                
                foreach (var delivery in selectedDeliveries)
                {
                    var sender = await _userRepository.GetByIdAsync(delivery.SenderId);
                    var trip = driverTrips.FirstOrDefault(t => t.TripId == delivery.TripId);
                    
                    if (trip != null)
                    {
                        var driver = await _userRepository.GetByIdAsync(trip.DriverId);
                        responseDtos.Add(MapToResponseDto(
                            delivery, 
                            sender?.FirstName + " " + sender?.LastName,
                            driver?.FirstName + " " + driver?.LastName
                        ));
                    }
                }
                
                return ApiResponse<List<DeliveryRequestResponseDto>>.SuccessResponse(responseDtos);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<DeliveryRequestResponseDto>>.ErrorResponse($"Error retrieving selected deliveries: {ex.Message}");
            }
        }

        private DeliveryRequestResponseDto MapToResponseDto(DeliveryRequest request, string? senderName, string? driverName = null)
        {
            // Try to parse the status enum
            Enum.TryParse<DeliveryStatus>(request.Status, out var statusEnum);
            
            // Get trip details if available
            var tripDetails = request.Trip != null ? new
            {
                SourceLocation = request.Trip.SourceLocation,
                Destination = request.Trip.Destination,
                StartTime = request.Trip.StartTime,
                Description = request.Trip.TripDescription,
                EstimatedDuration = request.Trip.EstimatedDuration
            } : null;
            
            // Get driver details if available
            string? driverId = null;
            string? driverPhone = null;
            string? driverProfileImage = null;
            double? driverRating = null;
            
            if (request.Trip?.Driver != null)
            {
                var driver = request.Trip.Driver;
                driverId = driver.Id;
                driverPhone = driver.PhoneNumber;
                driverProfileImage = driver.DrivingLicenseImage;
                
                // Use the AvgRating property directly
                driverRating = driver.AvgRating;
            }
            
            // Get sender details
            string? senderPhone = null;
            string? senderProfileImage = null;
            double? senderRating = null;
            
            if (request.Sender != null)
            {
                senderPhone = request.Sender.PhoneNumber;
                senderProfileImage = request.Sender.NationalIdImage;
                
                // Use the AvgRating property directly
                senderRating = request.Sender.AvgRating;
            }
            
            // Calculate timestamps based on status history (simplified implementation)
            DateTime? acceptedAt = null;
            DateTime? pickedUpAt = null;
            DateTime? deliveredAt = null;
            
            // In a real implementation, you would store these timestamps in the database
            // This is a simplified approach
            if (statusEnum >= DeliveryStatus.Accepted)
            {
                // If status is Accepted or further, assume it was accepted recently
                acceptedAt = request.AcceptedAt ?? DateTime.UtcNow.AddHours(-1); // Use stored timestamp or example
            }
            
            if (statusEnum >= DeliveryStatus.InTransit)
            {
                // If status is InTransit or further, assume it was picked up recently
                pickedUpAt = request.PickedUpAt ?? DateTime.UtcNow.AddMinutes(-30); // Use stored timestamp or example
            }
            
            if (statusEnum == DeliveryStatus.Delivered)
            {
                // If status is Delivered, assume it was delivered recently
                deliveredAt = request.DeliveredAt ?? DateTime.UtcNow.AddMinutes(-5); // Use stored timestamp or example
            }
            
            return new DeliveryRequestResponseDto
            {
                Id = request.Id,
                ReceiverPhone = request.ReceiverPhone,
                DropoffLocation = request.DropoffLocation,
                SourceLocation = request.SourceLocation,
                Weight = request.Weight,
                ItemDescription = request.ItemDescription,
                Price = request.Price,
                Status = statusEnum,
                SenderId = request.SenderId,
                SenderName = senderName ?? "Unknown",
                SenderPhone = senderPhone,
                SenderProfileImage = senderProfileImage,
                SenderRating = senderRating,
                TripId = request.TripId,
                DriverId = driverId,
                DriverName = driverName,
                DriverPhone = driverPhone,
                DriverProfileImage = driverProfileImage,
                DriverRating = driverRating,
                EstimatedDeliveryTime = request.Trip?.StartTime,
                EstimatedDuration = tripDetails?.EstimatedDuration,
                DeliveryStartDate = request.DeliveryStartDate,
                DeliveryEndDate = request.DeliveryEndDate,
                CreatedAt = request.CreatedAt,
                AcceptedAt = acceptedAt,
                PickedUpAt = pickedUpAt,
                DeliveredAt = deliveredAt,
                TripSourceLocation = tripDetails?.SourceLocation,
                TripDestination = tripDetails?.Destination,
                TripStartTime = tripDetails?.StartTime,
                TripDescription = tripDetails?.Description,
                DeliveryNotes = request.DeliveryNotes
            };
        }

        private bool IsValidStatusTransition(DeliveryStatus currentStatus, DeliveryStatus newStatus)
        {
            return (currentStatus, newStatus) switch
            {
                (DeliveryStatus.Pending, DeliveryStatus.TripSelected) => true,
                (DeliveryStatus.Pending, DeliveryStatus.Accepted) => true,
                (DeliveryStatus.Pending, DeliveryStatus.Rejected) => true,
                (DeliveryStatus.TripSelected, DeliveryStatus.Accepted) => true,
                (DeliveryStatus.TripSelected, DeliveryStatus.Rejected) => true,
                (DeliveryStatus.TripSelected, DeliveryStatus.Cancelled) => true,
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