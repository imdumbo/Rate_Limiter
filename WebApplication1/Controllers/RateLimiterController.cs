using Microsoft.AspNetCore.Mvc;
using WebApplication1.Model;
using WebApplication1.Process;
using WebApplication1.Process.Contract;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RateLimiterController : ControllerBase
    {
        private readonly IBatchProcessor _batchProcessor;
        private readonly IFileWriterService _fileWriterService;
        private readonly ILogger<RateLimiterController> _logger;

        public RateLimiterController(IBatchProcessor batchProcessor, IFileWriterService fileWriterService, ILogger<RateLimiterController> logger)
        {
            _batchProcessor = batchProcessor;
            _fileWriterService = fileWriterService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Start()
        {
            try
            {
                var action = new RequestContext() { WriteAsync = _fileWriterService.WriteFileAsync };

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
