namespace WebApplication1.Model
{
    public class RequestContext
    {
        public Func<int, Task> RequestDelegate { get; set; }
    }
}
