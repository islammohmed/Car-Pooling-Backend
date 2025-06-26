using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CarPooling.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RouteController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly string _orsKey = "5b3ce3597851110001cf62485624081c51de42f492e757034c8f0fff";

        public RouteController()
        {
            _httpClient = new HttpClient();
        }

        [HttpGet("route")]
        public async Task<IActionResult> GetRoute([FromQuery] string start, [FromQuery] string end)
        {
            var url = $"https://api.openrouteservice.org/v2/directions/driving-car?api_key={_orsKey}&start=8.681495,49.41461&end=8.687872,49.420318";
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("Authorization", _orsKey);

            var response = await _httpClient.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();
            return Content(json, "application/json");
        }

    }
}
