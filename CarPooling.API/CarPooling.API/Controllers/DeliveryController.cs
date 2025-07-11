using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CarPooling.Application.Interfaces;
using System.Security.Claims;
using CarPooling.Domain.DTOs;
using CarPooling.Application.DTOs;
using CarPooling.Application.Trips.DTOs;
using CarPooling.Application.Trips;

namespace CarPooling.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DeliveryController : ControllerBase
    {
        private readonly IDeliveryService _deliveryService;
        private readonly ITripService _tripService;
        private readonly ILogger<DeliveryController> _logger;

        public DeliveryController(IDeliveryService deliveryService, ITripService tripService, ILogger<DeliveryController> logger)
        {
            _deliveryService = deliveryService;
            _tripService = tripService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<DeliveryRequestResponseDto>>> CreateDeliveryRequest(CreateDeliveryRequestDto requestDto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<DeliveryRequestResponseDto>.ErrorResponse("User not authenticated"));
                }

                // Validate date range
                if (requestDto.DeliveryEndDate < DateTime.UtcNow)
                {
                    return BadRequest(ApiResponse<DeliveryRequestResponseDto>.ErrorResponse("Delivery end date cannot be in the past"));
                }

                var result = await _deliveryService.CreateRequestAsync(userId, requestDto);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating delivery request");
                return StatusCode(500, ApiResponse<DeliveryRequestResponseDto>.ErrorResponse($"An error occurred: {ex.Message}"));
            }
        }

        [HttpGet("matching-trips/{requestId}")]
        public async Task<ActionResult<ApiResponse<List<TripListDto>>>> GetMatchingTrips(int requestId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<List<TripListDto>>.ErrorResponse("User not authenticated"));
                }

                var result = await _deliveryService.GetMatchingTripsAsync(requestId);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting matching trips");
                return StatusCode(500, ApiResponse<List<TripListDto>>.ErrorResponse($"An error occurred: {ex.Message}"));
            }
        }

        [HttpGet("pending")]
        [Authorize(Roles = "Driver")]
        public async Task<ActionResult<ApiResponse<List<DeliveryRequestResponseDto>>>> GetPendingRequests()
        {
            try
            {
                // First, check and update expired requests
                await _deliveryService.HandleExpiredRequestsAsync();
                
                // Then get the valid pending requests
                var result = await _deliveryService.GetPendingRequestsAsync();
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pending delivery requests");
                return StatusCode(500, ApiResponse<List<DeliveryRequestResponseDto>>.ErrorResponse($"An error occurred: {ex.Message}"));
            }
        }

        [HttpGet("my-requests")]
        public async Task<ActionResult<ApiResponse<List<DeliveryRequestResponseDto>>>> GetMyRequests()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<List<DeliveryRequestResponseDto>>.ErrorResponse("User not authenticated"));
                }

                // Check for expired requests first
                await _deliveryService.HandleExpiredRequestsAsync();

                var result = await _deliveryService.GetUserRequestsAsync(userId);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user delivery requests");
                return StatusCode(500, ApiResponse<List<DeliveryRequestResponseDto>>.ErrorResponse($"An error occurred: {ex.Message}"));
            }
        }

        [HttpGet("my-deliveries")]
        [Authorize(Roles = "Driver")]
        public async Task<ActionResult<ApiResponse<List<DeliveryRequestResponseDto>>>> GetMyDeliveries()
        {
            try
            {
                var driverId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(driverId))
                {
                    return Unauthorized(ApiResponse<List<DeliveryRequestResponseDto>>.ErrorResponse("User not authenticated"));
                }

                var result = await _deliveryService.GetDriverDeliveriesAsync(driverId);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting driver deliveries");
                return StatusCode(500, ApiResponse<List<DeliveryRequestResponseDto>>.ErrorResponse($"An error occurred: {ex.Message}"));
            }
        }

        [HttpGet("selected-for-me")]
        [Authorize(Roles = "Driver")]
        public async Task<ActionResult<ApiResponse<List<DeliveryRequestResponseDto>>>> GetSelectedDeliveriesForDriver()
        {
            try
            {
                var driverId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(driverId))
                {
                    return Unauthorized(ApiResponse<List<DeliveryRequestResponseDto>>.ErrorResponse("User not authenticated"));
                }

                // Check for expired requests first
                await _deliveryService.HandleExpiredRequestsAsync();

                var result = await _deliveryService.GetSelectedDeliveriesForDriverAsync(driverId);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting selected deliveries for driver");
                return StatusCode(500, ApiResponse<List<DeliveryRequestResponseDto>>.ErrorResponse($"An error occurred: {ex.Message}"));
            }
        }

        [HttpPost("{requestId}/accept")]
        [Authorize(Roles = "Driver")]
        public async Task<ActionResult<ApiResponse<DeliveryRequestResponseDto>>> AcceptDelivery(int requestId, [FromQuery] int tripId)
        {
            try
            {
                var driverId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(driverId))
                {
                    return Unauthorized(ApiResponse<DeliveryRequestResponseDto>.ErrorResponse("User not authenticated"));
                }

                // Check if request is expired before accepting
                await _deliveryService.HandleExpiredRequestsAsync();

                var result = await _deliveryService.AcceptDeliveryAsync(driverId, requestId, tripId);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error accepting delivery");
                return StatusCode(500, ApiResponse<DeliveryRequestResponseDto>.ErrorResponse($"An error occurred: {ex.Message}"));
            }
        }

        [HttpPost("{requestId}/reject")]
        [Authorize(Roles = "Driver")]
        public async Task<ActionResult<ApiResponse<DeliveryRequestResponseDto>>> RejectDelivery(int requestId)
        {
            try
            {
                var driverId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(driverId))
                {
                    return Unauthorized(ApiResponse<DeliveryRequestResponseDto>.ErrorResponse("User not authenticated"));
                }

                // Check if request is expired before rejecting
                await _deliveryService.HandleExpiredRequestsAsync();

                var result = await _deliveryService.RejectDeliveryAsync(driverId, requestId);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting delivery");
                return StatusCode(500, ApiResponse<DeliveryRequestResponseDto>.ErrorResponse($"An error occurred: {ex.Message}"));
            }
        }

        [HttpPut("{requestId}/status")]
        [Authorize(Roles = "Driver")]
        public async Task<ActionResult<ApiResponse<DeliveryRequestResponseDto>>> UpdateDeliveryStatus(
            int requestId,
            UpdateDeliveryStatusDto updateDto)
        {
            try
            {
                var driverId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(driverId))
                {
                    return Unauthorized(ApiResponse<DeliveryRequestResponseDto>.ErrorResponse("User not authenticated"));
                }

                var result = await _deliveryService.UpdateDeliveryStatusAsync(driverId, requestId, updateDto);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating delivery status");
                return StatusCode(500, ApiResponse<DeliveryRequestResponseDto>.ErrorResponse($"An error occurred: {ex.Message}"));
            }
        }

        [HttpPost("{requestId}/cancel")]
        public async Task<ActionResult<ApiResponse<bool>>> CancelRequest(int requestId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<bool>.ErrorResponse("User not authenticated"));
                }

                var result = await _deliveryService.CancelRequestAsync(userId, requestId);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error canceling delivery request");
                return StatusCode(500, ApiResponse<bool>.ErrorResponse($"An error occurred: {ex.Message}"));
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<DeliveryRequestResponseDto>>> GetDeliveryRequest(int id)
        {
            try
            {
                var result = await _deliveryService.GetRequestByIdAsync(id);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting delivery request by ID");
                return StatusCode(500, ApiResponse<DeliveryRequestResponseDto>.ErrorResponse($"An error occurred: {ex.Message}"));
            }
        }
        
        [HttpPost("{requestId}/select-trip")]
        public async Task<ActionResult<ApiResponse<DeliveryRequestResponseDto>>> SelectTripForDelivery(int requestId, SelectTripForDeliveryDto selectTripDto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<DeliveryRequestResponseDto>.ErrorResponse("User not authenticated"));
                }

                // Check if request is expired before selecting a trip
                await _deliveryService.HandleExpiredRequestsAsync();

                var result = await _deliveryService.SelectTripForDeliveryAsync(userId, requestId, selectTripDto);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error selecting trip for delivery");
                return StatusCode(500, ApiResponse<DeliveryRequestResponseDto>.ErrorResponse($"An error occurred: {ex.Message}"));
            }
        }
        
        [HttpPost("check-expired")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<int>>> CheckAndHandleExpiredRequests()
        {
            try
            {
                var count = await _deliveryService.HandleExpiredRequestsAsync();
                return Ok(ApiResponse<int>.SuccessResponse(count, "Successfully processed expired delivery requests"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling expired requests");
                return StatusCode(500, ApiResponse<int>.ErrorResponse($"An error occurred: {ex.Message}"));
            }
        }
    }
} 