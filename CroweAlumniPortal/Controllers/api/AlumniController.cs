using CroweAlumniPortal.Data.IServices;
using Microsoft.AspNetCore.Mvc;

namespace CroweAlumniPortal.Controllers.api
{
    [Route("api/[controller]")]
    [ApiController]
    public class AlumniController : ControllerBase
    {
        private readonly IAlumniService alumniService;
        public AlumniController(IAlumniService alumniService) => this.alumniService = alumniService;

        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            var list = await alumniService.GetAllAlumniAsync();
            return Ok(list);
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 12)
        {
            var result = await alumniService.GetAlumniAsync(search, page, pageSize);
            return Ok(result);
        }

    }
}
