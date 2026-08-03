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
            var action = new RequestContext();
            action.WriteAsync = fileWriterService.WriteFileAsync;

            string fileName = $"RequestLog_{DateTime.Now:yyyyMMddHHmmss}";

            // Create N work items as Func<Task> and capture loop variable correctly
            int N = 100_000;
            var workItems = Enumerable.Range(1, N)
                .Select(i => (Func<Task>)(() => action.ExecuteAsync(fileName, i)))
                .ToList();

            // Create limiter via your static helper and pass it explicitly to BatchProcessor
            var limiter = RateLimiter.CreateTokenBucketRateLimiter();
            var batchProcessor = new BatchProcessor<Func<Task>>(limiter)
            {
                ProcessBatchAsync = async batch =>
                {
                    // Start all tasks in the batch and await them together
                    var running = batch.Select(f => f());
                    await Task.WhenAll(running);
                }
            };

            try
            {
                // Execute in batches (e.g., 100 per batch)
                await batchProcessor.ExecuteAsync(workItems, batchSize: 100);
            }
            finally
            {
                // dispose limiter when finished (caller-created)
                limiter.Dispose();
            }
            return Ok("Rate Limiting is working!");
        }
    }
}
