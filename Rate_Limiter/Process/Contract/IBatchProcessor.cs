using Rate_Limiter.Model;

namespace Rate_Limiter.Process.Contract
{
    public interface IBatchProcessor : IDisposable
    {
        int MaxDegreeOfParallelism { get; set; }
        Task ExecuteAsync(IEnumerable<int> items, RequestContext request, CancellationToken cancellationToken = default);
    }
}
