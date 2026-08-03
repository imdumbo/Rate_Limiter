using System.Threading.RateLimiting;

namespace WebApplication1.Process
{
    public static class RateLimiter
    {
        public static TokenBucketRateLimiter CreateTokenBucketRateLimiter()
        {
            // Define a bucket that gains 60 tokens every 1 seconds, max capacity int.MaxValue
            var limiter = new TokenBucketRateLimiter(new TokenBucketRateLimiterOptions
            {
                TokenLimit = int.MaxValue,
                ReplenishmentPeriod = TimeSpan.FromMinutes(1),
                TokensPerPeriod = 60,
                QueueLimit = 30
            });
            return limiter;
        }
    }
}
