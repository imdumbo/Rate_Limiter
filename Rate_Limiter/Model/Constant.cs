namespace Rate_Limiter.Model
{
    using System.IO;

    public static class Constant
    {
        // path to the wwwRoot folder relative to the current working directory.
        public static string FILEPATH => Path.Combine(Directory.GetCurrentDirectory(), "wwwRoot");
    }
}
