using Microsoft.AspNetCore.Mvc;

namespace CroweAlumniPortal.Controllers.api
{
    
        [ApiController]
        [Route("api/[controller]")]
        public class LocationController : ControllerBase
        {
            private readonly HttpClient _http;

            public LocationController(HttpClient http)
            {
                _http = http;
            }

            [HttpGet("countries")]
            public async Task<IActionResult> GetCountries()
            {
                // 🔴 External API – yahan se real data aa raha hoga
                var url = "https://countriesnow.space/api/v0.1/countries";

                HttpResponseMessage resp;
                try
                {
                    resp = await _http.GetAsync(url);
                }
                catch
                {
                    return StatusCode(502, "Failed to reach countries API.");
                }

                if (!resp.IsSuccessStatusCode)
                {
                    return StatusCode((int)resp.StatusCode, "Countries API returned error.");
                }

                var json = await resp.Content.ReadAsStringAsync();

                // 🔁 Forward JSON as-is so your JS stays same
                return Content(json, "application/json");
            }
        }
}
