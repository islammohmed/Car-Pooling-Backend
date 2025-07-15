using CarPooling.Application.Trips.DTOs;
using CarPooling.Application.DTOs;
using CarPooling.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using CarPooling.Application.Interfaces;
using CarPooling.Domain.DTOs;
using CarPooling.Domain.Exceptions;
using FluentValidation;

namespace CarPooling.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TripController : ControllerBase
    {
        private readonly IBookTripService _bookTripService;
        private readonly ITripService _tripService;
        private readonly IUserService _userService;
        private readonly ILogger<TripController> _logger;

        public TripController(IBookTripService bookTripService, ITripService tripService, IUserService userService, ILogger<TripController> logger)
        {
            _bookTripService = bookTripService;
            _tripService = tripService;
            _userService = userService;
            _logger = logger;
        }

        [HttpGet]
        [AllowAnonymous] // Temporarily allow anonymous access for testing
        public async Task<ActionResult<PaginatedResponse<TripListDto>>> GetAllTrips([FromQuery] PaginationParams paginationParams)
        {
            try
            {
                // Check and update trip statuses
                await _tripService.UpdateTripStatusAsync(0); // 0 means check all trips
                
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                var result = await _tripService.GetAllTripsAsync(paginationParams);
                
                // Add information about whether the current user is booked on each trip
                if (!string.IsNullOrEmpty(userId))
                {
                    foreach (var trip in result.Items)
                    {
                        var isBookedResponse = await _tripService.IsUserBookedOnTripAsync(userId, trip.TripId);
                        if (isBookedResponse.Success)
                        {
                            trip.IsUserBooked = isBookedResponse.Data;
                        }
                    }
                }
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all trips");
                return StatusCode(500, ApiResponse<PaginatedResponse<TripListDto>>.ErrorResponse($"An error occurred: {ex.Message}"));
            }
        }

        [HttpGet("search")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<TripListDto>>> SearchTrips(
            [FromQuery] string source,
            [FromQuery] string destination,
            [FromQuery] string date)
        {
            try
            {
                if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(destination) || string.IsNullOrEmpty(date))
                {
                    return BadRequest(ApiResponse<IEnumerable<TripListDto>>.ErrorResponse("Source, destination, and date are required parameters"));
                }

                // Parse date
                if (!DateTime.TryParse(date, out DateTime tripDate))
                {
                    return BadRequest(ApiResponse<IEnumerable<TripListDto>>.ErrorResponse("Invalid date format"));
                }

                var filteredTrips = await _tripService.SearchTripsAsync(source, destination, tripDate);

                return Ok(filteredTrips);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching trips");
                return StatusCode(500, ApiResponse<IEnumerable<TripListDto>>.ErrorResponse($"An error occurred: {ex.Message}"));
            }
        }

        [HttpGet("my-trips")]
        [Authorize(Roles = "Driver")]
        public async Task<ActionResult<IEnumerable<TripListDto>>> GetMyTrips()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<IEnumerable<TripListDto>>.ErrorResponse("User not authenticated"));
                }

                // Use pagination params with a large page size to get all trips
                var paginationParams = new PaginationParams { PageNumber = 1, PageSize = 100 };
                var allTripsResult = await _tripService.GetAllTripsAsync(paginationParams);
                
                // Since TripListDto doesn't have DriverId, we need to get trips from the repository
                var myTrips = await _tripService.GetUserTripsAsync(userId);
                return Ok(myTrips);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting driver trips");
                return StatusCode(500, ApiResponse<IEnumerable<TripListDto>>.ErrorResponse($"An error occurred: {ex.Message}"));
            }
        }

        [HttpGet("my-bookings")]
        [Authorize(Roles = "Passenger")]
        public async Task<ActionResult<IEnumerable<TripListDto>>> GetMyBookings()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<IEnumerable<TripListDto>>.ErrorResponse("User not authenticated"));
                }

                // Get all trips where the user is a participant
                var myBookings = await _tripService.GetUserBookingsAsync(userId);
                return Ok(myBookings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting passenger bookings");
                return StatusCode(500, ApiResponse<IEnumerable<TripListDto>>.ErrorResponse($"An error occurred: {ex.Message}"));
            }
        }

        [HttpGet("check-booking/{tripId}")]
        public async Task<ActionResult<ApiResponse<bool>>> CheckUserBooking(int tripId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<bool>.ErrorResponse("User not authenticated"));
                }

                var result = await _tripService.IsUserBookedOnTripAsync(userId, tripId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking user booking status");
                return StatusCode(500, ApiResponse<bool>.ErrorResponse($"An error occurred: {ex.Message}"));
            }
        }

        [HttpPost]
        [Authorize(Roles = "Driver")]
        public async Task<ActionResult<TripListDto>> CreateTrip([FromBody] CreateTripDto tripDto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<TripListDto>.ErrorResponse("User not authenticated"));
                }
                
                // Check if driver's documents are verified
                var documentVerificationsResult = await _userService.GetUserDocumentVerifications(userId);
                if (!documentVerificationsResult.Success)
                {
                    return BadRequest(documentVerificationsResult.Message);
                }

                var documentVerifications = documentVerificationsResult.Data;
                
                // Check if all required documents are verified
                bool hasVerifiedNationalId = documentVerifications?.Any(
                    doc => doc.DocumentType == "NationalId" && doc.Status == VerificationStatus.Approved
                ) ?? false;
                
                bool hasVerifiedDrivingLicense = documentVerifications?.Any(
                    doc => doc.DocumentType == "DrivingLicense" && doc.Status == VerificationStatus.Approved
                ) ?? false;
                
                bool hasVerifiedCarLicense = documentVerifications?.Any(
                    doc => doc.DocumentType == "CarLicense" && doc.Status == VerificationStatus.Approved
                ) ?? false;
                
                if (!hasVerifiedNationalId || !hasVerifiedDrivingLicense || !hasVerifiedCarLicense)
                {
                    return BadRequest(ApiResponse<TripListDto>.ErrorResponse("You must verify your documents before posting a trip."));
                }
                
                tripDto.DriverId = userId;
                var tripId = await _tripService.CreateTripAsync(tripDto);
                var trip = await _tripService.GetTripByIdAsync(tripId);
                
                if (trip == null)
                {
                    return NotFound(ApiResponse<TripListDto>.ErrorResponse($"Trip with ID {tripId} was not found after creation."));
                }

                return CreatedAtAction(nameof(GetTripById), new { id = tripId }, trip);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating trip");
                return StatusCode(500, ApiResponse<TripListDto>.ErrorResponse($"An error occurred: {ex.Message}"));
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TripListDto>> GetTripById(int id)
        {
            try
            {
                // Check and update trip status
                await _tripService.UpdateTripStatusAsync(id);
                
                var trip = await _tripService.GetTripByIdAsync(id);
                
                if (trip == null)
                {
                    return NotFound(ApiResponse<TripListDto>.ErrorResponse($"Trip with ID {id} was not found."));
                }
                
                // Get trip participants
                var participants = await _tripService.GetTripParticipantsAsync(id);
                
                // Get current user ID if authenticated
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                // Check if current user is booked on this trip
                if (!string.IsNullOrEmpty(userId))
                {
                    var isBookedResponse = await _tripService.IsUserBookedOnTripAsync(userId, id);
                    if (isBookedResponse.Success)
                    {
                        trip.IsUserBooked = isBookedResponse.Data;
                    }
                }
                
                // Get driver details
                var driverDetailsResponse = await _userService.GetUserByIdAsync(trip.DriverId);
                
                // Create a response object with trip details, participants, and driver details
                var responseData = new
                {
                    Trip = trip,
                    Participants = participants,
                    ParticipantCount = participants.Count,
                    Driver = driverDetailsResponse.Success ? driverDetailsResponse.Data : null
                };

                // Create the API response
                var apiResponse = new
                {
                    Success = true,
                    Message = "Trip details retrieved successfully",
                    Data = responseData,
                    Errors = Array.Empty<string>()
                };

                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting trip by ID");
                return StatusCode(500, ApiResponse<TripListDto>.ErrorResponse($"An error occurred: {ex.Message}"));
            }
        }

        [HttpPost("book")]
        [Authorize(Roles = "Passenger,Driver")]
        public async Task<IActionResult> BookTrip([FromBody] BookTripDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not authenticated");
            }
            
            // Check and update trip status
            await _tripService.UpdateTripStatusAsync(dto.TripId);
            
            dto.UserId = userId;

            try
            {
                // If user is a driver, check if they can book this trip
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
                if (userRole == UserRole.Driver.ToString())
                {
                    var canBookResult = await _bookTripService.CanDriverBookTripAsync(userId, dto.TripId);
                    if (!canBookResult.Success)
                    {
                        return BadRequest(canBookResult);
                    }
                }
                
                var result = await _bookTripService.BookTrip(dto);
                
                // After booking, check if trip is now full and update status
                await _tripService.UpdateTripStatusAsync(dto.TripId);
                
                return Ok(result);
            }
            catch (TripBookingException ex)
            {
                // Return a 400 Bad Request with the specific error message
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (ValidationException ex)
            {
                // Handle validation errors
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        [HttpPost("cancel/passenger")]
        [Authorize(Roles = "Passenger")]
        public async Task<IActionResult> CancelTripAsPassenger([FromBody] CancelTripDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not authenticated");
            }

            dto.UserId = userId;
            dto.Role = UserRole.Passenger;

            try
            {
                var result = await _bookTripService.CancelTrip(dto);
                
                // After cancellation, update trip status
                await _tripService.UpdateTripStatusAsync(dto.TripId);
                
                return Ok(new { Success = result, Message = "Trip cancelled successfully by passenger" });
            }
            catch (TripBookingException ex)
            {
                // Return a 400 Bad Request with the specific error message
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (ValidationException ex)
            {
                // Handle validation errors
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        [HttpPost("cancel/driver")]
        [Authorize(Roles = "Driver")]
        public async Task<IActionResult> CancelTripAsDriver([FromBody] CancelTripDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not authenticated");
            }

            dto.UserId = userId;
            dto.Role = UserRole.Driver;

            try
            {
                var result = await _bookTripService.CancelTrip(dto);
                return Ok(new { Success = result, Message = "Trip cancelled successfully by driver" });
            }
            catch (TripBookingException ex)
            {
                // Return a 400 Bad Request with the specific error message
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (ValidationException ex)
            {
                // Handle validation errors
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }
        
        [HttpPost("complete/{tripId}")]
        [Authorize(Roles = "Driver")]
        public async Task<ActionResult<ApiResponse<TripListDto>>> CompleteTrip(int tripId)
        {
            try
            {
                var driverId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(driverId))
                {
                    return Unauthorized(ApiResponse<TripListDto>.ErrorResponse("User not authenticated"));
                }
                
                var result = await _tripService.CompleteTripAsync(tripId, driverId);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing trip");
                return StatusCode(500, ApiResponse<TripListDto>.ErrorResponse($"An error occurred: {ex.Message}"));
            }
        }
        
        [HttpPost("update-status/{tripId}")]
        public async Task<ActionResult<ApiResponse<bool>>> UpdateTripStatus(int tripId)
        {
            try
            {
                var result = await _tripService.UpdateTripStatusAsync(tripId);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating trip status");
                return StatusCode(500, ApiResponse<bool>.ErrorResponse($"An error occurred: {ex.Message}"));
            }
        }
        
        [HttpGet("{tripId}/participants")]
        public async Task<ActionResult<ApiResponse<List<TripParticipantDto>>>> GetTripParticipants(int tripId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse<List<TripParticipantDto>>.ErrorResponse("User not authenticated"));
                }
                
                // Check if the trip exists
                var trip = await _tripService.GetTripByIdAsync(tripId);
                if (trip == null)
                {
                    return NotFound(ApiResponse<List<TripParticipantDto>>.ErrorResponse($"Trip with ID {tripId} was not found."));
                }
                
                // Get participants for the trip
                var participants = await _tripService.GetTripParticipantsAsync(tripId);
                
                return Ok(ApiResponse<List<TripParticipantDto>>.SuccessResponse(participants));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting trip participants");
                return StatusCode(500, ApiResponse<List<TripParticipantDto>>.ErrorResponse($"An error occurred: {ex.Message}"));
            }
        }
    }
}
