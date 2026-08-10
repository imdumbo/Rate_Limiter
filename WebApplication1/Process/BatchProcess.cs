using System;
using System.IO;
using System.Threading.RateLimiting;
using Microsoft.Extensions.Logging;
using WebApplication1.Model;

namespace WebApplication1.Process
{
    /// <summary>
    /// Generic batch processor that invokes a configured processor for each batch of items.
    /// </summary>
    public class BatchProcessor : IDisposable, WebApplication1.Process.Contract.IBatchProcessor
    {
        private readonly WebApplication1.Process.Contract.IRpmTracker _rpmTracker;
        private readonly ILogger<BatchProcessor>? _logger;

        // Default rate limiter configured: 100 tokens per second, queues excess requests
        // Tune TokensPerPeriod & QueueLimit based on downstream throughput
        private static readonly TokenBucketRateLimiter _limiter = new TokenBucketRateLimiter(
            new TokenBucketRateLimiterOptions
            {
                TokenLimit = 100,
                TokensPerPeriod = 100,
                ReplenishmentPeriod = TimeSpan.FromSeconds(1),  // ← FIXED: Was 1 MINUTE, now 1 SECOND
                AutoReplenishment = true,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 1000  // ← FIXED: Was 0 (reject), now allows queuing
            });

        public int MaxDegreeOfParallelism { get; set; } = 1;
        private readonly string _fullPath;

        public BatchProcessor(WebApplication1.Process.Contract.IRpmTracker rpmTracker, ILogger<BatchProcessor>? logger = null)
        {
            _rpmTracker = rpmTracker;
            _logger = logger;
            _fullPath = Path.Combine(Model.Constant.FILEPATH, $"RequestLog_{DateTime.Now:yyyyMMddHHmmss}.txt");
        }

        public async Task ExecuteAsync(IEnumerable<int> items, RequestContext request, CancellationToken cancellationToken = default)
        {
            var options = new ParallelOptions
            {
                MaxDegreeOfParallelism = MaxDegreeOfParallelism,
                CancellationToken = cancellationToken
            };

            long rejectedCount = 0;
            long successCount = 0;

            // Parallel.ForEachAsync automatically handles concurrency limits safely.
            // No manual batching or Task.Run needed!
            await Parallel.ForEachAsync(items, options, async (element, ct) =>
            {
                // Wait for the rate limiter bucket
                using var lease = await _limiter.AcquireAsync(1, ct);

                if (lease.IsAcquired)
                {
                    try
                    {
                        await request.ExecuteAsync(_fullPath, element);
                        _rpmTracker.TrackCall();
                        Interlocked.Increment(ref successCount);
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError(ex, "Error processing request {RequestNumber}", element);
                    }
                }
                else
                {
                    Interlocked.Increment(ref rejectedCount);
                    _logger?.LogWarning("Rate limiter rejected request {RequestNumber}. Queue may be full.", element);
                }
            });

            _logger?.LogInformation("Batch processing complete. Successful: {SuccessCount}, Rejected: {RejectedCount}", 
                successCount, rejectedCount);
        }

        public void Dispose()
        {
            // Do not dispose injected singleton IRpmTracker here; DI container will manage its lifetime.
            // Keep empty to satisfy IDisposable contract.
        }
    }
}