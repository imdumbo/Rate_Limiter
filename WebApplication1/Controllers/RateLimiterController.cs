using Microsoft.AspNetCore.Mvc;
using System.Linq;
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

            BatchProcessor batchProcessor = new BatchProcessor(RateLimiter.CreateTokenBucketRateLimiter(), true);
            var requests = Enumerable.Range(1, 1_00_000);

            await batchProcessor.ExecuteAsync(requests, action, 100);
            
            return Ok("Rate Limiting is working!");
        }
    }
}
