using CroweAlumniPortal.Helper;
using CroweAlumniPortal.Models;
using Microsoft.AspNetCore.Mvc;

namespace CroweAlumniPortal.Controllers.api
{
    [Route("api/[controller]")]
    [ApiController]
    public class MetaController : ControllerBase
    {
        [HttpGet("user-approval-status")]
        public IActionResult GetUserApprovalStatus()
        {
            var data = EnumHelper.ToDataset<UserApprovalStatus>();
            return Ok(data);
        }
    }
}
