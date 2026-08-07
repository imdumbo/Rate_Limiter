namespace WebApplication1.Process
{
    public class FileWriterService
    {
        // a static semaphore lock shared across all instances
        private static readonly SemaphoreSlim _fileLock = new SemaphoreSlim(1, 1);

        public async Task WriteFileAsync(string fullPath, int num)
        {
            // Make threads wait in line asynchronously before accessing the file
            await _fileLock.WaitAsync();
            try
            {
                // Write to the file safely
                using (StreamWriter writer = new StreamWriter(fullPath, append: true))
                {
                    await Task.Delay(100);
                    await writer.WriteLineAsync($"Request {num}");
                }
            }
            finally
            {
                // CRITICAL: Always release the lock so the next thread can go
                _fileLock.Release();
            }
        }
    }
}
