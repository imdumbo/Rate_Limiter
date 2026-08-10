namespace WebApplication1.Process.Contract
{
    public interface IFileWriterService
    {
        Task WriteFileAsync(string fullPath, int num);
    }
}
