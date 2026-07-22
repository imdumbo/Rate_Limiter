namespace WebApplication1.Process
{
    public class FileWriterService
    {
        public Task WriteFileAsync()
        {
            string fileName = $"{Model.Constant.FILEPATH}RequestLog_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
            // check file exists or not
            if(File.Exists(fileName))
            {
                // if exists delete it
                File.Delete(fileName);
            }
            // and if not create a new file
            using (System.IO.StreamWriter writer = new System.IO.StreamWriter(fileName))
            {
                for(int i = 0; i < 10; i++)
                {
                    writer.WriteLine($"Request {i + 1}");
                }
            }
            return Task.CompletedTask;
        }
    }
}
