using CarPooling.Application.Trips.Commands.CreateRequest;
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

        public TripController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> CreateTrip([FromBody] CreateTripCommand command)
        {
            int id = await _mediator.Send(command);
            return Ok(id);
           // return CreatedAtAction(nameof(GetByID), new { id }, null);
        }
    }
}
