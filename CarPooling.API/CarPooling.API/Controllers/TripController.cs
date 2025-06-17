using Azure.Core;
using CarPooling.Application.Trips;
using CarPooling.Application.Trips.Commands.CreateRequest;
using CarPooling.Application.Trips.DTOs;
using MediatR;
using Microsoft.AspNetCore.Http;
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

        //bookig a trip

        [HttpPost("book")]
        public async Task<IActionResult> BookTrip([FromBody] BookTripDto dto)
        {
            var result = await _bookTripService.BookTripAsync(dto);
            return Ok(result);
        }
    }
}
