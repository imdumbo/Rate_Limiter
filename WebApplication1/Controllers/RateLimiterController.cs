using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RateLimiterController : ControllerBase
    {
        [HttpGet]
        public IActionResult Index()
        {
            return Ok("Rate Limiting is working!");
        }
    }
}
