namespace WebApplication1.Process
{
    public class FileWriterService : WebApplication1.Process.Contract.IFileWriterService
    {
        // a static semaphore lock shared across all instances
        private static readonly SemaphoreSlim _fileLock = new SemaphoreSlim(1, 1);
        private static readonly TimeZoneInfo istZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");

        public async Task WriteFileAsync(string fullPath, int num)
        {   
            // Make threads wait in line asynchronously before accessing the file
            await _fileLock.WaitAsync();
            try
            {
                // Write to the file safely
                using (StreamWriter writer = new StreamWriter(fullPath, append: true))
                {
                    DateTime istTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, istZone);
                    int delay = Random.Shared.Next(10, 1001);
                    await Task.Delay(delay);
                    await writer.WriteLineAsync($"Request {num} written at {istTime:yyyy-MM-dd HH:mm:ss} IST with delay of {delay} ms");
                }
            }
            catch (Exception ex)
            {
                // Log locally - consider adding ILogger via DI if needed
                System.Diagnostics.Debug.WriteLine($"Error writing request {num}: {ex.Message}");
                throw;
            }
            finally
            {
                // CRITICAL: Always release the lock so the next thread can go
                // This ensures no deadlock even if an exception occurs above
                _fileLock.Release();
            }
        }
    }
}

