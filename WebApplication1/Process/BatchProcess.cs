using System.Threading.RateLimiting;

namespace WebApplication1.Process
{
    /// <summary>
    /// Generic batch processor that invokes a configured processor for each batch of items.
    /// </summary>
    public class BatchProcessor<T> : IDisposable
    {
        public Func<List<T>, Task>? ProcessBatchAsync { get; set; }

        // Token bucket limiter (required). Caller must provide an instance to avoid accidental defaults.
        public TokenBucketRateLimiter limiter { get; }

        private readonly RateMeter _rateMeter = new RateMeter();
        private readonly Timer? _logTimer;

        public int MaxDegreeOfParallelism { get; set; } = 1;

        /// <summary>
        /// Construct a BatchProcessor with a required TokenBucketRateLimiter. Do not pass null.
        /// If enablePeriodicConsoleLogging is true, RPM will be written to the console every minute.
        /// </summary>
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
            }
            catch
            {
                // swallow logging errors
            }
        }

        public async Task ExecuteAsync(IEnumerable<T> items, int batchSize, CancellationToken cancellationToken = default)
        {
            if (ProcessBatchAsync == null)
                throw new InvalidOperationException("No processor configured.");

            if (batchSize <= 0) throw new ArgumentOutOfRangeException(nameof(batchSize));
            if (MaxDegreeOfParallelism <= 0) MaxDegreeOfParallelism = 1;

            var batches = new List<List<T>>();
            var current = new List<T>(batchSize);
            foreach (var item in items)
            {
                cancellationToken.ThrowIfCancellationRequested();
                current.Add(item);
                if (current.Count == batchSize)
                {
                    batches.Add(current);
                    current = new List<T>(batchSize);
                }
            }
            if (current.Count > 0) batches.Add(current);

            using var sem = new SemaphoreSlim(MaxDegreeOfParallelism);
            var running = new List<Task>(Math.Min(batches.Count, MaxDegreeOfParallelism));

            foreach (var batch in batches)
            {
                await sem.WaitAsync(cancellationToken).ConfigureAwait(false);

                var task = Task.Run(async () =>
                {
                    try
                    {
                        // If limiter is configured, acquire permits equal to batch size before processing
                        if (limiter != null)
                        {
                            while (true)
                            {
                                cancellationToken.ThrowIfCancellationRequested();
                                using var lease = await limiter.AcquireAsync(batch.Count, cancellationToken).ConfigureAwait(false);
                                if (lease.IsAcquired) break;
                                var retry = TimeSpan.FromMilliseconds(100);
                                await Task.Delay(retry, cancellationToken).ConfigureAwait(false);
                            }
                        }

                        await ProcessBatchAsync(batch).ConfigureAwait(false);

                        // mark processed items and log to console
                        _rateMeter.Mark(batch.Count);
                        Console.WriteLine($"Processed batch of {batch.Count}. RPM: {_rateMeter.GetRpm()}");
                    }
                    finally
                    {
                        sem.Release();
                    }
                }, cancellationToken);

                running.Add(task);
                running.RemoveAll(t => t.IsCompleted);
            }

            await Task.WhenAll(running).ConfigureAwait(false);
        }

        public void Dispose()
        {
            try { _logTimer?.Dispose(); } catch { }
        }
    }
}
