using CarPooling.Application.DTOs;
using CarPooling.Application.Common.Results;
using CarPooling.Application.Interfaces;
using CarPooling.Domain.Entities;
using CarPooling.Domain.Enums;
using CarPooling.Domain.DTOs;
using CarPooling.Domain.Interfaces;
using CarPooling.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using CarPooling.Application.Trips.DTOs;
using System.Text.RegularExpressions;

namespace CarPooling.Application.Services
{
    public class DeliveryService : IDeliveryService
    {
        private readonly IDeliveryRequestRepository _deliveryRepository;
        private readonly IUserRepository _userRepository;
        private readonly ITripRepository _tripRepository;
        private readonly ILogger<DeliveryService> _logger;

        public DeliveryService(
            IDeliveryRequestRepository deliveryRepository,
            IUserRepository userRepository,
            ITripRepository tripRepository,
            ILogger<DeliveryService> logger)
        {
            _deliveryRepository = deliveryRepository;
            _userRepository = userRepository;
            _tripRepository = tripRepository;
            _logger = logger;
        }

        public async Task<Result<DeliveryRequestResponseDto>> CreateRequestAsync(string userId, CreateDeliveryRequestDto requestDto)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return Result<DeliveryRequestResponseDto>.Failure("User not found");
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
                    Status = DeliveryStatus.Pending,
                    DeliveryStartDate = requestDto.DeliveryStartDate,
                    DeliveryEndDate = requestDto.DeliveryEndDate,
                    CreatedAt = DateTime.UtcNow
                };

                var created = await _deliveryRepository.CreateAsync(deliveryRequest);
                return Result<DeliveryRequestResponseDto>.Success(MapToResponseDto(created, user.FirstName + " " + user.LastName));
            }
            catch (Exception ex)
            {
                return Result<DeliveryRequestResponseDto>.Failure($"Error creating delivery request: {ex.Message}");
            }
        }

        public async Task<Result<DeliveryRequestResponseDto>> AcceptDeliveryAsync(string driverId, int requestId, int tripId)
        {
            try
            {
                var driver = await _userRepository.GetByIdAsync(driverId);
                if (driver == null || driver.UserRole != UserRole.Driver)
                {
                    return Result<DeliveryRequestResponseDto>.Failure("Invalid driver");
                }

                var trip = await _tripRepository.GetByIdAsync(tripId);
                if (trip == null || trip.DriverId != driverId)
                {
                    return Result<DeliveryRequestResponseDto>.Failure("Invalid trip");
                }

                if (!trip.AcceptsDeliveries)
                {
                    return Result<DeliveryRequestResponseDto>.Failure("This trip does not accept deliveries");
                }

                var request = await _deliveryRepository.GetByIdAsync(requestId);
                if (request == null)
                {
                    return Result<DeliveryRequestResponseDto>.Failure("Delivery request not found");
                }

                // Accept both Pending and TripSelected statuses
                if (request.Status != DeliveryStatus.Pending && request.Status != DeliveryStatus.TripSelected)
                {
                    return Result<DeliveryRequestResponseDto>.Failure("Request is not in a state that can be accepted");
                }

                // For TripSelected status, verify the trip matches the one selected by the user
                if (request.Status == DeliveryStatus.TripSelected && request.TripId != tripId)
                {
                    return Result<DeliveryRequestResponseDto>.Failure("This delivery request was selected for a different trip");
                }

                // Check if request is expired
                if (request.DeliveryEndDate < DateTime.UtcNow)
                {
                    // Automatically update the status to cancelled
                    request.Status = DeliveryStatus.Cancelled;
                    await _deliveryRepository.UpdateAsync(request);
                    return Result<DeliveryRequestResponseDto>.Failure("This delivery request has expired and has been cancelled");
                }

                // Only verify location match if the status is Pending (not needed for TripSelected as it was already verified)
                if (request.Status == DeliveryStatus.Pending)
                {
                    if (!IsLocationMatch(trip.SourceLocation, request.SourceLocation) ||
                        !IsLocationMatch(trip.Destination, request.DropoffLocation))
                    {
                        return Result<DeliveryRequestResponseDto>.Failure(
                            "Delivery source and destination must match the trip's route");
                    }

                    if (trip.MaxDeliveryWeight.HasValue && request.Weight > trip.MaxDeliveryWeight.Value)
                    {
                        return Result<DeliveryRequestResponseDto>.Failure(
                            $"Package weight exceeds trip's maximum delivery weight of {trip.MaxDeliveryWeight.Value}kg");
                    }
                }

                if (trip.StartTime < DateTime.UtcNow)
                {
                    return Result<DeliveryRequestResponseDto>.Failure("Cannot accept delivery for a past trip");
                }

                if (trip.Status != TripStatus.Pending && trip.Status != TripStatus.Confirmed)
                {
                    return Result<DeliveryRequestResponseDto>.Failure(
                        "Can only accept deliveries for pending or confirmed trips");
                }

                request.Status = DeliveryStatus.Accepted;
                request.TripId = tripId;
                request.AcceptedAt = DateTime.UtcNow;

                var updated = await _deliveryRepository.UpdateAsync(request);
                return Result<DeliveryRequestResponseDto>.Success(
                    MapToResponseDto(updated, request.Sender.FirstName + " " + request.Sender.LastName,
                        driver.FirstName + " " + driver.LastName));
            }
            catch (Exception ex)
            {
                return Result<DeliveryRequestResponseDto>.Failure($"Error accepting delivery: {ex.Message}");
            }
        }

        public async Task<Result<DeliveryRequestResponseDto>> UpdateDeliveryStatusAsync(string driverId, int requestId, UpdateDeliveryStatusDto updateDto)
        {
            try
            {
                var request = await _deliveryRepository.GetByIdAsync(requestId);
                if (request == null)
                {
                    return Result<DeliveryRequestResponseDto>.Failure("Delivery request not found");
                }

                if (!request.TripId.HasValue)
                {
                    return Result<DeliveryRequestResponseDto>.Failure("This delivery has not been accepted by any driver");
                }

                var trip = await _tripRepository.GetByIdAsync(request.TripId.Value);
                if (trip == null || trip.DriverId != driverId)
                {
                    return Result<DeliveryRequestResponseDto>.Failure("Unauthorized to update this delivery");
                }

                if (!Enum.TryParse<DeliveryStatus>(updateDto.Status, out var newStatus))
                {
                    return Result<DeliveryRequestResponseDto>.Failure("Invalid status");
                }

                var currentStatus = request.Status;

                if (!IsValidStatusTransition(currentStatus, newStatus))
                {
                    return Result<DeliveryRequestResponseDto>.Failure("Invalid status transition");
                }

                // Update the status
                request.Status = newStatus;

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
                return Result<DeliveryRequestResponseDto>.Success(
                    MapToResponseDto(updated, request.Sender.FirstName + " " + request.Sender.LastName, driver?.FirstName + " " + driver?.LastName));
            }
            catch (Exception ex)
            {
                return Result<DeliveryRequestResponseDto>.Failure($"Error updating delivery status: {ex.Message}");
            }
        }

        public async Task<Result<bool>> CancelRequestAsync(string userId, int requestId)
        {
            try
            {
                var request = await _deliveryRepository.GetByIdAsync(requestId);
                if (request == null)
                {
                    return Result<bool>.Failure("Delivery request not found");
                }

                if (request.SenderId != userId)
                {
                    return Result<bool>.Failure("Unauthorized to cancel this request");
                }

                if (request.Status != DeliveryStatus.Pending && request.Status != DeliveryStatus.TripSelected)
                {
                    return Result<bool>.Failure("Can only cancel pending or trip-selected requests");
                }

                request.Status = DeliveryStatus.Cancelled;
                await _deliveryRepository.UpdateAsync(request);
                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return Result<bool>.Failure($"Error cancelling request: {ex.Message}");
            }
        }

        public async Task<Result<DeliveryRequestResponseDto>> GetRequestByIdAsync(int id)
        {
            try
            {
                var request = await _deliveryRepository.GetByIdAsync(id);
                if (request == null)
                {
                    return Result<DeliveryRequestResponseDto>.Failure("Delivery request not found");
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

                return Result<DeliveryRequestResponseDto>.Success(
                    MapToResponseDto(request, sender?.FirstName + " " + sender?.LastName, driverName));
            }
            catch (Exception ex)
            {
                return Result<DeliveryRequestResponseDto>.Failure($"Error retrieving delivery request: {ex.Message}");
            }
        }

        public async Task<Result<List<DeliveryRequestResponseDto>>> GetPendingRequestsAsync()
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
                        request.Status = DeliveryStatus.Cancelled;
                        await _deliveryRepository.UpdateAsync(request);
                        // Skip adding this to the response
                        continue;
                    }

                    var sender = await _userRepository.GetByIdAsync(request.SenderId);
                    responseDtos.Add(MapToResponseDto(request, sender?.FirstName + " " + sender?.LastName));
                }

                return Result<List<DeliveryRequestResponseDto>>.Success(responseDtos);
            }
            catch (Exception ex)
            {
                return Result<List<DeliveryRequestResponseDto>>.Failure($"Error retrieving pending requests: {ex.Message}");
            }
        }

        public async Task<Result<List<DeliveryRequestResponseDto>>> GetUserRequestsAsync(string userId)
        {
            try
            {
                var requests = await _deliveryRepository.GetUserRequestsAsync(userId);
                var responseDtos = new List<DeliveryRequestResponseDto>();

                foreach (var request in requests)
                {
                    // Check if pending request is expired
                    if (request.Status == DeliveryStatus.Pending && request.DeliveryEndDate < DateTime.UtcNow)
                    {
                        // Update status to cancelled for expired requests
                        request.Status = DeliveryStatus.Cancelled;
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

                return Result<List<DeliveryRequestResponseDto>>.Success(responseDtos);
            }
            catch (Exception ex)
            {
                return Result<List<DeliveryRequestResponseDto>>.Failure($"Error retrieving user requests: {ex.Message}");
            }
        }

        public async Task<Result<List<DeliveryRequestResponseDto>>> GetDriverDeliveriesAsync(string driverId)
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

                return Result<List<DeliveryRequestResponseDto>>.Success(responseDtos);
            }
            catch (Exception ex)
            {
                return Result<List<DeliveryRequestResponseDto>>.Failure($"Error retrieving driver deliveries: {ex.Message}");
            }
        }

        public async Task<Result<List<TripListDto>>> GetMatchingTripsAsync(int requestId)
        {
            try
            {
                var request = await _deliveryRepository.GetByIdAsync(requestId);
                if (request == null)
                {
                    return Result<List<TripListDto>>.Failure("Delivery request not found");
                }

                // Only check for expiration if the request is still in Pending status
                if (request.Status == DeliveryStatus.Pending && request.DeliveryEndDate < DateTime.UtcNow)
                {
                    // Automatically update the status to cancelled
                    request.Status = DeliveryStatus.Cancelled;
                    await _deliveryRepository.UpdateAsync(request);
                    return Result<List<TripListDto>>.Failure("This delivery request has expired and has been cancelled");
                }

                // If request is not in Pending status, it can't match with trips
                if (request.Status != DeliveryStatus.Pending)
                {
                    return Result<List<TripListDto>>.Failure($"Cannot find matching trips for a request with status: {request.Status}");
                }

                _logger.LogInformation($"Finding matching trips for delivery request {requestId} from {request.SourceLocation} to {request.DropoffLocation}");

                // Get all trips
                var (trips, _) = await _tripRepository.GetAllTripsAsync(1, 1000);

                // Filter trips that match our criteria
                var matchingTrips = trips
                    .Where(t => t.AcceptsDeliveries &&
                               IsLocationMatch(t.SourceLocation, request.SourceLocation) &&
                               IsLocationMatch(t.Destination, request.DropoffLocation) &&
                               (!t.MaxDeliveryWeight.HasValue || t.MaxDeliveryWeight.Value >= request.Weight) &&
                               (t.Status == TripStatus.Pending || t.Status == TripStatus.Confirmed) &&
                               t.StartTime > DateTime.UtcNow &&
                               t.StartTime >= request.DeliveryStartDate &&
                               t.StartTime <= request.DeliveryEndDate)
                    .ToList();

                _logger.LogInformation($"Found {matchingTrips.Count} matching trips for delivery request {requestId}");

                // Map the trips to DTOs
                var tripDtos = matchingTrips.Select(trip => new TripListDto
                {
                    TripId = trip.TripId,
                    DriverId = trip.DriverId,
                    DriverName = $"{trip.Driver?.FirstName ?? "Unknown"} {trip.Driver?.LastName ?? "Driver"}",
                    PricePerSeat = trip.PricePerSeat,
                    EstimatedDuration = trip.EstimatedDuration,
                    AvailableSeats = trip.AvailableSeats,
                    TripDescription = trip.TripDescription ?? string.Empty,
                    Status = trip.Status,
                    SourceLocation = trip.SourceLocation,
                    Destination = trip.Destination,
                    StartTime = trip.StartTime,
                    CreatedAt = trip.CreatedAt,
                    GenderPreference = trip.GenderPreference,
                    ParticipantsCount = trip.Participants?.Count ?? 0
                }).ToList();

                return Result<List<TripListDto>>.Success(tripDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding matching trips for delivery request {RequestId}: {Message}", requestId, ex.Message);
                return Result<List<TripListDto>>.Failure($"Error finding matching trips: {ex.Message}");
            }
        }

        public async Task<Result<DeliveryRequestResponseDto>> SelectTripForDeliveryAsync(string userId, int requestId, SelectTripForDeliveryDto selectTripDto)
        {
            try
            {
                var request = await _deliveryRepository.GetByIdAsync(requestId);
                if (request == null)
                {
                    return Result<DeliveryRequestResponseDto>.Failure("Delivery request not found");
                }

                if (request.SenderId != userId)
                {
                    return Result<DeliveryRequestResponseDto>.Failure("Unauthorized to select a trip for this delivery request");
                }

                if (request.Status != DeliveryStatus.Pending)
                {
                    return Result<DeliveryRequestResponseDto>.Failure("Can only select a trip for pending requests");
                }

                // Check if request is expired
                if (request.DeliveryEndDate < DateTime.UtcNow)
                {
                    // Automatically update the status to cancelled
                    request.Status = DeliveryStatus.Cancelled;
                    await _deliveryRepository.UpdateAsync(request);
                    return Result<DeliveryRequestResponseDto>.Failure("This delivery request has expired and has been cancelled");
                }

                // Verify the trip exists and can accept deliveries
                var trip = await _tripRepository.GetByIdAsync(selectTripDto.TripId);
                if (trip == null)
                {
                    return Result<DeliveryRequestResponseDto>.Failure("Trip not found");
                }

                if (!trip.AcceptsDeliveries)
                {
                    return Result<DeliveryRequestResponseDto>.Failure("This trip does not accept deliveries");
                }

                if (trip.StartTime < DateTime.UtcNow)
                {
                    return Result<DeliveryRequestResponseDto>.Failure("Cannot select a past trip");
                }

                if (trip.Status != TripStatus.Pending && trip.Status != TripStatus.Confirmed)
                {
                    return Result<DeliveryRequestResponseDto>.Failure(
                        "Can only select pending or confirmed trips");
                }

                // Verify locations match
                if (!IsLocationMatch(trip.SourceLocation, request.SourceLocation) ||
                    !IsLocationMatch(trip.Destination, request.DropoffLocation))
                {
                    return Result<DeliveryRequestResponseDto>.Failure(
                        "Delivery source and destination must match the trip's route");
                }

                // Verify weight is acceptable
                if (trip.MaxDeliveryWeight.HasValue && request.Weight > trip.MaxDeliveryWeight.Value)
                {
                    return Result<DeliveryRequestResponseDto>.Failure(
                        $"Package weight exceeds trip's maximum delivery weight of {trip.MaxDeliveryWeight.Value}kg");
                }

                // Update the request with the selected trip
                request.TripId = selectTripDto.TripId;
                request.Status = DeliveryStatus.TripSelected;

                // Store delivery notes if provided
                if (!string.IsNullOrEmpty(selectTripDto.DeliveryNotes))
                {
                    request.DeliveryNotes = selectTripDto.DeliveryNotes;
                }

                var updated = await _deliveryRepository.UpdateAsync(request);

                // Get driver name for response
                var driver = await _userRepository.GetByIdAsync(trip.DriverId);
                var driverName = driver != null ? $"{driver.FirstName} {driver.LastName}" : null;

                return Result<DeliveryRequestResponseDto>.Success(
                    MapToResponseDto(updated, request.Sender.FirstName + " " + request.Sender.LastName, driverName));
            }
            catch (Exception ex)
            {
                return Result<DeliveryRequestResponseDto>.Failure($"Error selecting trip for delivery: {ex.Message}");
            }
        }

        public async Task<Result<int>> HandleExpiredRequestsAsync()
        {
            try
            {
                // Get all pending requests
                var pendingRequests = await _deliveryRepository.GetPendingRequestsAsync();

                // Filter for expired requests (where the end date has passed)
                var expiredRequests = pendingRequests
                    .Where(r => (r.Status == DeliveryStatus.Pending ||
                                r.Status == DeliveryStatus.TripSelected) &&
                           r.DeliveryEndDate < DateTime.UtcNow)
                    .ToList();

                // Update status to cancelled for all expired requests
                foreach (var request in expiredRequests)
                {
                    request.Status = DeliveryStatus.Cancelled;
                    await _deliveryRepository.UpdateAsync(request);
                }

                return Result<int>.Success(expiredRequests.Count, "Expired requests handled successfully");
            }
            catch (Exception ex)
            {
                // Log the error but do not throw, as this is a background operation.
                _logger.LogError(ex, "Error handling expired delivery requests");
                return Result<int>.Failure("Error handling expired delivery requests");
            }
        }

        public async Task<Result<DeliveryRequestResponseDto>> RejectDeliveryAsync(string driverId, int requestId)
        {
            try
            {
                var driver = await _userRepository.GetByIdAsync(driverId);
                if (driver == null || driver.UserRole != UserRole.Driver)
                {
                    return Result<DeliveryRequestResponseDto>.Failure("Invalid driver");
                }

                var request = await _deliveryRepository.GetByIdAsync(requestId);
                if (request == null)
                {
                    return Result<DeliveryRequestResponseDto>.Failure("Delivery request not found");
                }

                // Can only reject TripSelected requests that are associated with this driver's trip
                if (request.Status != DeliveryStatus.TripSelected)
                {
                    return Result<DeliveryRequestResponseDto>.Failure("Can only reject requests that have been selected for a trip");
                }

                if (!request.TripId.HasValue)
                {
                    return Result<DeliveryRequestResponseDto>.Failure("This request is not associated with any trip");
                }

                var trip = await _tripRepository.GetByIdAsync(request.TripId.Value);
                if (trip == null || trip.DriverId != driverId)
                {
                    return Result<DeliveryRequestResponseDto>.Failure("Unauthorized to reject this delivery request");
                }

                request.Status = DeliveryStatus.Rejected;
                // Clear the TripId to allow the user to select another trip
                request.TripId = null;

                var updated = await _deliveryRepository.UpdateAsync(request);
                return Result<DeliveryRequestResponseDto>.Success(
                    MapToResponseDto(updated, request.Sender.FirstName + " " + request.Sender.LastName));
            }
            catch (Exception ex)
            {
                return Result<DeliveryRequestResponseDto>.Failure($"Error rejecting delivery: {ex.Message}");
            }
        }

        public async Task<Result<List<DeliveryRequestResponseDto>>> GetSelectedDeliveriesForDriverAsync(string driverId)
        {
            try
            {
                // Get all trips owned by this driver
                var driverTrips = await _tripRepository.GetDriverTripsAsync(driverId);

                if (driverTrips == null || !driverTrips.Any())
                {
                    return Result<List<DeliveryRequestResponseDto>>.Success(new List<DeliveryRequestResponseDto>());
                }

                // Get all delivery requests that are in TripSelected status and associated with any of the driver's trips
                var tripIds = driverTrips.Select(t => t.TripId).ToList();
                var selectedDeliveries = await _deliveryRepository.GetDeliveriesByStatusAndTripsAsync(DeliveryStatus.TripSelected, tripIds);

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

                return Result<List<DeliveryRequestResponseDto>>.Success(responseDtos);
            }
            catch (Exception ex)
            {
                return Result<List<DeliveryRequestResponseDto>>.Failure($"Error retrieving selected deliveries: {ex.Message}");
            }
        }

        private DeliveryRequestResponseDto MapToResponseDto(DeliveryRequest request, string? senderName, string? driverName = null)
        {
            // Try to parse the status enum
            var statusEnum = request.Status; 
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
            if (string.IsNullOrWhiteSpace(location1) || string.IsNullOrWhiteSpace(location2))
                return false;

            // Normalize strings: trim, lowercase, and remove extra spaces
            string normalized1 = NormalizeString(location1);
            string normalized2 = NormalizeString(location2);

            // 1. Exact match after normalization
            if (normalized1 == normalized2)
                return true;

            // 2. Check if one contains the other
            if (normalized1.Contains(normalized2) || normalized2.Contains(normalized1))
                return true;

            // 3. Check for common words (useful for addresses with multiple parts)
            var words1 = normalized1.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var words2 = normalized2.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            // If locations share at least 2 significant words, consider them a match
            int commonWords = words1.Intersect(words2).Count();
            if (commonWords >= 2)
                return true;

            // 4. Check for similar words using Levenshtein distance
            // This helps with misspellings
            foreach (var word1 in words1)
            {
                if (word1.Length <= 3) continue; // Skip short words

                foreach (var word2 in words2)
                {
                    if (word2.Length <= 3) continue; // Skip short words

                    if (LevenshteinDistance(word1, word2) <= 2)
                        return true;
                }
            }

            return false;
        }

        private string NormalizeString(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            // Convert to lowercase
            string result = input.ToLowerInvariant();

            // Remove punctuation
            result = Regex.Replace(result, @"[^\w\s]", " ");

            // Replace multiple spaces with a single space
            result = Regex.Replace(result, @"\s+", " ");

            // Trim leading and trailing spaces
            return result.Trim();
        }

        private int LevenshteinDistance(string s, string t)
        {
            // Levenshtein distance algorithm to calculate edit distance between strings
            // This helps identify similar strings with typos

            int n = s.Length;
            int m = t.Length;
            int[,] d = new int[n + 1, m + 1];

            if (n == 0) return m;
            if (m == 0) return n;

            for (int i = 0; i <= n; i++) d[i, 0] = i;
            for (int j = 0; j <= m; j++) d[0, j] = j;

            for (int i = 1; i <= n; i++)
            {
                for (int j = 1; j <= m; j++)
                {
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }

            return d[n, m];
        }
    }
}