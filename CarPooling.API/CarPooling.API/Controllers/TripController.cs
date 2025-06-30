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
        private IMediator _mediator;
        private IBookTripService _bookTripService;

        public TripController(IMediator mediator, IBookTripService bookTripService)
        {
            _mediator = mediator;
            _bookTripService = bookTripService;

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
