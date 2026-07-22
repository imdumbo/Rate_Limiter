using Microsoft.AspNetCore.Mvc;
using WebApplication1.Process;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RateLimiterController : ControllerBase
    {
        [HttpGet]
        public IActionResult Start()
        {
            FileWriterService fileWriterService = new FileWriterService();
            fileWriterService.WriteFileAsync();
            return Ok("Rate Limiting is working!");
        }
    }
}
