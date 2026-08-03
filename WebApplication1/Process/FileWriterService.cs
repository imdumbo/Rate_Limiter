namespace WebApplication1.Process
{
    public class FileWriterService
    {
        public async Task WriteFileAsync(string fileName, int num)
        {
            fileName = $"{Model.Constant.FILEPATH}{fileName}.txt";
            // write into the file
            using (System.IO.StreamWriter writer = new System.IO.StreamWriter(fileName, append: true))
            {
                //for (int i = 0; i < num; i++)
                //{
                //   await writer.WriteLineAsync($"Request {i + 1}");
                //}
                await writer.WriteLineAsync($"Request {num}");
            }
            return;
        }
    }
}
