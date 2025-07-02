using CarPooling.Application.Trips;
using CarPooling.Application.Trips.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace CarPooling.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
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
        public async Task<IActionResult> CreateTrip([FromBody] CreateTripDto tripDto)
        {
            int id = await _tripService.CreateTripAsync(tripDto);
            return Ok(id);
        }

        //booking a trip

        [HttpPost("book")]
        public async Task<IActionResult> BookTrip([FromBody] BookTripDto dto)
        {
            var result = await _bookTripService.BookTrip(dto);
            return Ok(result);
        }
        [HttpPost("cancel")]
        public async Task<IActionResult> CancelTrip(CancelTripDto dto)
        {
            var result = await _bookTripService.CancelTrip(dto);
            return Ok(new { Success = result, Message = "Trip cancelled successfully" });
        }

    }
}
