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

        public DeliveryController(IDeliveryService deliveryService, ITripService tripService)
        {
            _deliveryService = deliveryService;
            _tripService = tripService;
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<DeliveryRequestResponseDto>>> CreateDeliveryRequest(CreateDeliveryRequestDto requestDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponse<DeliveryRequestResponseDto>.ErrorResponse("User not authenticated"));
            }

            var result = await _deliveryService.CreateRequestAsync(userId, requestDto);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("matching-trips/{requestId}")]
        public async Task<ActionResult<ApiResponse<List<TripListDto>>>> GetMatchingTrips(int requestId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponse<List<TripListDto>>.ErrorResponse("User not authenticated"));
            }

            var result = await _deliveryService.GetMatchingTripsAsync(requestId);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("pending")]
        [Authorize(Roles = "Driver")]
        public async Task<ActionResult<ApiResponse<List<DeliveryRequestResponseDto>>>> GetPendingRequests()
        {
            var result = await _deliveryService.GetPendingRequestsAsync();
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("my-requests")]
        public async Task<ActionResult<ApiResponse<List<DeliveryRequestResponseDto>>>> GetMyRequests()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponse<List<DeliveryRequestResponseDto>>.ErrorResponse("User not authenticated"));
            }

            var result = await _deliveryService.GetUserRequestsAsync(userId);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("my-deliveries")]
        [Authorize(Roles = "Driver")]
        public async Task<ActionResult<ApiResponse<List<DeliveryRequestResponseDto>>>> GetMyDeliveries()
        {
            var driverId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(driverId))
            {
                return Unauthorized(ApiResponse<List<DeliveryRequestResponseDto>>.ErrorResponse("User not authenticated"));
            }

            var result = await _deliveryService.GetDriverDeliveriesAsync(driverId);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("{requestId}/accept")]
        [Authorize(Roles = "Driver")]
        public async Task<ActionResult<ApiResponse<DeliveryRequestResponseDto>>> AcceptDelivery(int requestId, [FromQuery] int tripId)
        {
            var driverId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(driverId))
            {
                return Unauthorized(ApiResponse<DeliveryRequestResponseDto>.ErrorResponse("User not authenticated"));
            }

            var result = await _deliveryService.AcceptDeliveryAsync(driverId, requestId, tripId);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPut("{requestId}/status")]
        [Authorize(Roles = "Driver")]
        public async Task<ActionResult<ApiResponse<DeliveryRequestResponseDto>>> UpdateDeliveryStatus(
            int requestId,
            UpdateDeliveryStatusDto updateDto)
        {
            var driverId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(driverId))
            {
                return Unauthorized(ApiResponse<DeliveryRequestResponseDto>.ErrorResponse("User not authenticated"));
            }

            var result = await _deliveryService.UpdateDeliveryStatusAsync(driverId, requestId, updateDto);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("{requestId}/cancel")]
        public async Task<ActionResult<ApiResponse<bool>>> CancelRequest(int requestId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponse<bool>.ErrorResponse("User not authenticated"));
            }

            var result = await _deliveryService.CancelRequestAsync(userId, requestId);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<DeliveryRequestResponseDto>>> GetDeliveryRequest(int id)
        {
            var result = await _deliveryService.GetRequestByIdAsync(id);
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
} 