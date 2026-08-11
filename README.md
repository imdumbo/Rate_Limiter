# 🚀 Rate Limiter

A production-ready **ASP.NET Core 10** rate limiting solution using token bucket algorithm for controlled batch processing of large request volumes.

**Key Capability:** Process 100,000+ requests with configurable throughput (default: 100 req/sec) with intelligent queue management.

## ✨ Quick Features

✅ **Token Bucket Rate Limiter** - Industry-standard throughput control  
✅ **Async/Await Throughout** - Non-blocking concurrent processing  
✅ **Batch Processing** - Handle 100k+ items efficiently  
✅ **Real-time Monitoring** - RPM tracking (logs every 60 seconds)  
✅ **Thread-Safe Logging** - Concurrent-safe file writes via semaphore  
✅ **Configurable** - Customize tokens/sec and queue limits  
✅ **Professional Error Handling** - Comprehensive logging and recovery  
✅ **Dependency Injection** - Full DI support for testability

## 🎯 Project Overview

This batch request processor enforces rate limits to:
- **Process large volumes** (100k+) without overwhelming downstream services
- **Control throughput** with token bucket algorithm (configurable req/sec)
- **Queue requests** intelligently instead of immediate rejection
- **Track performance** in real-time with RPM monitoring
- **Log safely** with thread-safe concurrent writes
- **Handle errors gracefully** with comprehensive logging

## 📦 Architecture

```
HTTP GET Request
	↓
[RateLimiterController]
	↓
[BatchProcessor]  ← Orchestrates processing
	├─→ [TokenBucketRateLimiter]  ← Controls throughput
	├─→ [RpmTracker]              ← Monitors requests/minute
	└─→ [FileWriterService]       ← Thread-safe logging
			↓
		RequestLog_*.txt
```

## ⚡ Performance

| Metric | Value |
|--------|-------|
| **Default Throughput** | 100 requests/second |
| **Queue Limit** | 1,000 pending requests |
| **Concurrency** | Configurable (default: sequential) |
| **Memory** | ~50 MB for 100k requests |
| **File I/O** | Serialized writes (no race conditions) |

## ⚙️ Configuration

**Rate Limiter Settings** in `Rate_Limiter/Process/BatchProcess.cs`:

```csharp
private static readonly TokenBucketRateLimiter _limiter = new TokenBucketRateLimiter(
    new TokenBucketRateLimiterOptions
    {
        TokenLimit = 100,                              // Max tokens
        TokensPerPeriod = 100,                         // Per second
        ReplenishmentPeriod = TimeSpan.FromSeconds(1), // Refill interval
        AutoReplenishment = true,
        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
        QueueLimit = 1000                              // Queue size
    });
```

### Tuning Examples

**High Throughput (500 req/sec):**
```csharp
TokensPerPeriod = 500,
QueueLimit = 5000
```

**Low Latency (10 req/sec):**
```csharp
TokensPerPeriod = 10,
TokenLimit = 20,
QueueLimit = 100
```

**Strict Rejection (no queue):**
```csharp
QueueLimit = 0  // Reject immediately if no tokens
```

## 🔧 Troubleshooting

| Issue | Cause | Solution |
|-------|-------|----------|
| **Only 100 requests process** | ReplenishmentPeriod = 1 MINUTE | Change to `FromSeconds(1)` |
| **RPM logs 0** | QueueLimit = 0 (rejecting all) | Change to `QueueLimit = 1000` |
| **File permission error** | Insufficient access rights | Run VS as Admin or grant permissions |
| **IST timezone not found** | Non-Windows system | Use UTC instead: `DateTime.UtcNow` |

## 🏃 Getting Started

### Prerequisites
- .NET 10 SDK
- Visual Studio 2026 Community (or any IDE)

### Installation & Run

```bash
# Clone and navigate
git clone https://github.com/imdumbo/Rate_Limiter.git
cd Rate_Limiter

# Restore and build
dotnet restore
dotnet build

# Run
dotnet run --project Rate_Limiter
```

### First Request

**Option 1: Browser**
```
http://localhost:5000/swagger
→ Click RateLimiterController
→ Try /api/ratelimiter GET endpoint
```

**Option 2: PowerShell**
```powershell
curl -X GET "http://localhost:5000/api/ratelimiter"
```

**Option 3: C# HttpClient**
```csharp
using var client = new HttpClient();
var response = await client.GetAsync("http://localhost:5000/api/ratelimiter");
var content = await response.Content.ReadAsStringAsync();
Console.WriteLine(content);
```

### Watch Progress

**Console Output:**
```
[2026-01-15 10:30:45] Starting batch processing of 100000 requests
[2026-01-15 10:30:45] Batch processing complete. Successful: 100000, Rejected: 0
[2026-01-15 10:31:45] [Server UTC] Server RPM: 100
```

**Output File:**
```
F:\Net Project\Rate Limiter\Rate_Limiter\Rate_Limiter\wwwRoot\RequestLog_*.txt

Request 1 written at 2026-01-15 10:30:45 IST with delay of 234 ms
Request 2 written at 2026-01-15 10:30:46 IST with delay of 512 ms
...
Request 100000 written at 2026-01-15 10:45:30 IST with delay of 789 ms
```
