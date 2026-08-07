namespace WebApplication1.Process
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public class RpmTracker : IDisposable
    {
        private long _requestCount;
        private readonly CancellationTokenSource _cts = new();

        public RpmTracker()
        {
            // 1. Create a dedicated background thread that ignores web traffic load
            var thread = new Thread(LogAndResetLoop)
            {
                IsBackground = true,
                Name = "RpmTrackerThread" // Helpful for debugging
            };
            thread.Start();
        }

        public void TrackCall()
        {
            Interlocked.Increment(ref _requestCount);
        }

        private void LogAndResetLoop()
        {
            // 2. WaitOne blocks this specific thread for exactly 1 minute.
            // It returns 'false' when the time elapses, and 'true' if the app shuts down (_cts is cancelled).
            while (!_cts.Token.WaitHandle.WaitOne(TimeSpan.FromMinutes(1)))
            {
                long currentRpm = Interlocked.Exchange(ref _requestCount, 0);

                Console.WriteLine($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC] Server RPM: {currentRpm}");
            }
        }

        public void Dispose()
        {
            _cts.Cancel();
            _cts.Dispose();
        }
    }
}