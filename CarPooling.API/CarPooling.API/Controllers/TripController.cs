using CarPooling.Application.Trips;
using CarPooling.Application.Trips.Commands.CreateRequest;
using CarPooling.Application.Trips.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CarPooling.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TripController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IBookTripService _bookTripService;
        private readonly ITripService _tripService;

        public TripController(IMediator mediator, IBookTripService bookTripService, ITripService tripService)
        {
            _mediator = mediator;
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
        public async Task<IActionResult> CreateTrip([FromBody] CreateTripCommand command)
        {
            int id = await _mediator.Send(command);
            return Ok(id);
            // return CreatedAtAction(nameof(GetByID), new { id }, null);
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
