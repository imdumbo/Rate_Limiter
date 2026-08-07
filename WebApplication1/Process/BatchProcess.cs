using System;
using System.IO;
using System.Threading.RateLimiting;
using WebApplication1.Model;

namespace WebApplication1.Process
{
    /// <summary>
    /// Generic batch processor that invokes a configured processor for each batch of items.
    /// </summary>
    public class BatchProcessor : IDisposable
    {
        private readonly RpmTracker _rpmTracker;
        // Default rate limiter configured with conservative defaults. Make this configurable if needed.
        private static readonly TokenBucketRateLimiter _limiter = new TokenBucketRateLimiter(
            new TokenBucketRateLimiterOptions
            {
                TokenLimit = 100,
                TokensPerPeriod = 100,
                ReplenishmentPeriod = TimeSpan.FromMinutes(1),
                AutoReplenishment = true,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            });

        public int MaxDegreeOfParallelism { get; set; } = 1;
        private readonly string _fullPath;
        public BatchProcessor()
        {
            _rpmTracker = new RpmTracker();
            _fullPath = Path.Combine(Model.Constant.FILEPATH, $"RequestLog_{DateTime.Now:yyyyMMddHHmmss}.txt");
        }

        public async Task ExecuteAsync(IEnumerable<int> items, RequestContext request, CancellationToken cancellationToken = default)
        {
            var options = new ParallelOptions
            {
                MaxDegreeOfParallelism = MaxDegreeOfParallelism,
                CancellationToken = cancellationToken
            };

            // Parallel.ForEachAsync automatically handles concurrency limits safely.
            // No manual batching or Task.Run needed!
            await Parallel.ForEachAsync(items, options, async (element, ct) =>
            {
                // Wait for the rate limiter bucket
                using var lease = await _limiter.AcquireAsync(1, ct);

                if (lease.IsAcquired)
                {
                    await request.ExecuteAsync(_fullPath, element);
                    _rpmTracker.TrackCall();
                }
            });
        }

        public void Dispose()
        {
            _rpmTracker.Dispose();
        }
    }
}