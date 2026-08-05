using System.Threading.RateLimiting;
using WebApplication1.Model;

namespace WebApplication1.Process
{
    /// <summary>
    /// Generic batch processor that invokes a configured processor for each batch of items.
    /// </summary>
    public class BatchProcessor : IDisposable
    {
        private string fileName = $"RequestLog_{DateTime.Now:yyyyMMddHHmmss}";
        // Token bucket limiter (required). Caller must provide an instance to avoid accidental defaults.
        public TokenBucketRateLimiter limiter { get; }

        private readonly RateMeter _rateMeter = new RateMeter();
        private readonly Timer? _logTimer;

        public int MaxDegreeOfParallelism { get; set; } = 1;

        public BatchProcessor(TokenBucketRateLimiter tokenBucketLimiter, bool enablePeriodicConsoleLogging = false)
        {
            limiter = tokenBucketLimiter ?? throw new ArgumentNullException(nameof(tokenBucketLimiter));
            if (enablePeriodicConsoleLogging)
            {
                _logTimer = new Timer(_ => LogRpm(), null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
            }
        }

        private void LogRpm()
        {
            try
            {
                var rpm = _rateMeter.GetRpm();
                Console.WriteLine($"RPM: {rpm}");
                // reset counts after logging to start a fresh minute
                _rateMeter.Reset();
            }
            catch
            {
                // swallow logging errors
            }
        }

        public async Task ExecuteAsync(IEnumerable<int> items, RequestContext request, int batchSize, CancellationToken cancellationToken = default)
        {
            var batches = createBatches(items, batchSize);
            var options = new ParallelOptions
            {
                MaxDegreeOfParallelism = MaxDegreeOfParallelism,
                CancellationToken = cancellationToken
            };
            var tasks = batches
                        .SelectMany(batch => batch.Select(element =>
                            Task.Run(async () =>
                            {
                                await limiter.AcquireAsync(1, cancellationToken).ConfigureAwait(false);
                                await request.ExecuteAsync(fileName, element).ConfigureAwait(false);
                                _rateMeter.Increment();
                            }, cancellationToken)))
                        .ToArray();

            await Task.WhenAll(tasks);
        }

        private IEnumerable<IEnumerable<int>> createBatches(IEnumerable<int> items, int batchSize)
        {
            return items.Select((item, index) => new { item, index })
                       .GroupBy(x => x.index / batchSize)
                       .Select(group => group.Select(x => x.item));
        }
        public void Dispose()
        {
            try { _logTimer?.Dispose(); } catch { }
        }
    }
}
