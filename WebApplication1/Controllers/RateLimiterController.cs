using Microsoft.AspNetCore.Mvc;
using WebApplication1.Model;
using WebApplication1.Process;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RateLimiterController : ControllerBase
    {
        private readonly BatchProcessor _batchProcessor;
        private readonly ILogger<RateLimiterController> _logger;

        public RateLimiterController(BatchProcessor batchProcessor, ILogger<RateLimiterController> logger)
        {
            _batchProcessor = batchProcessor;
            _logger = logger;
        }

        [HttpGet]
            public async Task<IActionResult> Start()
            {
                try
                {
                    // Create a generic logger factory to pass a non-generic logger
                    var fileWriterService = new FileWriterService();
                    var action = new RequestContext() { WriteAsync = fileWriterService.WriteFileAsync };

                    var requests = Enumerable.Range(1, 1_00_000);

                    _logger.LogInformation("Starting batch processing of {Count} requests", requests.Count());
                    await _batchProcessor.ExecuteAsync(requests, action, default);

                    _logger.LogInformation("Batch processing completed successfully");
                    return Ok("Rate Limiting is working!");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during batch processing");
                    return StatusCode(500, new { error = ex.Message });
                }
            }
    }
}
