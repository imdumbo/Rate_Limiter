using Microsoft.AspNetCore.Mvc;
using WebApplication1.Model;
using WebApplication1.Process;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RateLimiterController : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> Start()
        {
            FileWriterService fileWriterService = new FileWriterService();
            var action = new RequestContext() { WriteAsync = fileWriterService.WriteFileAsync };

            BatchProcessor batchProcessor = new BatchProcessor();
            var requests = Enumerable.Range(1, 1_00_000);

            await batchProcessor.ExecuteAsync(requests, action, default);
            
            return Ok("Rate Limiting is working!");
        }
    }
}
