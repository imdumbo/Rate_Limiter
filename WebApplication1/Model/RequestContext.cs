namespace WebApplication1.Model
{
    public class RequestContext
    {
        public Func<string, int, Task>? WriteAsync { get; set; }

        public async Task ExecuteAsync(string fileName, int num)
        {
            if (WriteAsync == null)
                throw new InvalidOperationException("No write method assigned.");

            await WriteAsync(fileName, num);
        }
    }
}
