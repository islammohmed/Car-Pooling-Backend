using CarPooling.Application.Trips;
using CarPooling.Application.Trips.DTOs;
using CarPooling.Application.DTOs;
using CarPooling.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CarPooling.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TripController : ControllerBase
    {
        private readonly IBookTripService _bookTripService;
        private readonly ITripService _tripService;

        public TripController(IBookTripService bookTripService, ITripService tripService)
        {
            _bookTripService = bookTripService;
            _tripService = tripService;
        }

        [HttpGet]
        public async Task<ActionResult<PaginatedResponse<TripListDto>>> GetAllTrips([FromQuery] PaginationParams paginationParams)
        {
            var result = await _tripService.GetAllTripsAsync(paginationParams);
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Driver")]
        public async Task<ActionResult<TripListDto>> CreateTrip([FromBody] CreateTripDto tripDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not authenticated");
            }
            
            tripDto.DriverId = userId;
            var tripId = await _tripService.CreateTripAsync(tripDto);
            var trip = await _tripService.GetTripByIdAsync(tripId);
            
            if (trip == null)
            {
                return NotFound($"Trip with ID {tripId} was not found after creation.");
            }

            return CreatedAtAction(nameof(GetTripById), new { id = tripId }, trip);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TripListDto>> GetTripById(int id)
        {
            var trip = await _tripService.GetTripByIdAsync(id);
            
            if (trip == null)
            {
                return NotFound($"Trip with ID {id} was not found.");
            }

            return Ok(trip);
        }

        [HttpPost("book")]
        [Authorize(Roles = "Passenger")]
        public async Task<IActionResult> BookTrip([FromBody] BookTripDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not authenticated");
            }
            dto.UserId = userId;

            var result = await _bookTripService.BookTrip(dto);
            return Ok(result);
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

            var result = await _bookTripService.CancelTrip(dto);
            return Ok(new { Success = result, Message = "Trip cancelled successfully by passenger" });
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

            var result = await _bookTripService.CancelTrip(dto);
            return Ok(new { Success = result, Message = "Trip cancelled successfully by driver" });
        }
    }
}
