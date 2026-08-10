namespace WebApplication1.Process.Contract
{
    public interface IRpmTracker : IDisposable
    {
        void TrackCall();
    }
}
