using System;
using System.Threading;

namespace WebApplication1.Process
{
    /// <summary>
    /// Lightweight per-minute rate meter using 60 per-second buckets.
    /// Thread-safe using Interlocked operations.
    /// </summary>
    public class RateMeter
    {
        private readonly long[] _counts = new long[60];
        private readonly long[] _timestamps = new long[60]; // seconds since epoch

        /// <summary>
        /// Mark occurrence(s). Default amount = 1.
        /// </summary>
        public void Mark(int amount = 1)
        {
            if (amount <= 0) return;
            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var idx = (int)(now % 60);

            var ts = Interlocked.Read(ref _timestamps[idx]);
            if (ts != now)
            {
                // new second; reset timestamp and set count
                Interlocked.Exchange(ref _timestamps[idx], now);
                Interlocked.Exchange(ref _counts[idx], amount);
            }
            else
            {
                Interlocked.Add(ref _counts[idx], amount);
            }
        }

        /// <summary>
        /// Convenience increment method used by callers.
        /// </summary>
        public void Increment() => Mark(1);

        /// <summary>
        /// Reset all internal buckets (counts and timestamps) to zero.
        /// After calling Reset, GetRpm() will return 0 until new marks arrive.
        /// Thread-safe using Interlocked operations.
        /// </summary>
        public void Reset()
        {
            for (int i = 0; i < 60; i++)
            {
                Interlocked.Exchange(ref _counts[i], 0);
                Interlocked.Exchange(ref _timestamps[i], 0);
            }
        }

        /// <summary>
        /// Get the total marks in the last 60 seconds (RPM approximation).
        /// </summary>
        public long GetRpm()
        {
            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            long sum = 0;
            for (int i = 0; i < 60; i++)
            {
                var ts = Interlocked.Read(ref _timestamps[i]);
                if (now - ts < 60)
                {
                    sum += Interlocked.Read(ref _counts[i]);
                }
            }
            return sum;
        }
    }
}
