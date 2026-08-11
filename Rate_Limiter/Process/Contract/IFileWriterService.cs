namespace Rate_Limiter.Process.Contract
{
    public interface IFileWriterService
    {
        Task WriteFileAsync(string fullPath, int num);
    }
}
