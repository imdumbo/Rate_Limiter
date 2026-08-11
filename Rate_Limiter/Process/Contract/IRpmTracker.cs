namespace Rate_Limiter.Process.Contract
{
    public interface IRpmTracker : IDisposable
    {
        void TrackCall();
    }
}
