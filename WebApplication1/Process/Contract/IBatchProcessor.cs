using WebApplication1.Model;

namespace WebApplication1.Process.Contract
{
    public interface IBatchProcessor : IDisposable
    {
        int MaxDegreeOfParallelism { get; set; }
        Task ExecuteAsync(IEnumerable<int> items, RequestContext request, CancellationToken cancellationToken = default);
    }
}
