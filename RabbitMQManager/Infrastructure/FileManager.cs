using System.IO;

namespace RabbitMQManager.Infrastructure
{
    public class FileManager
    {
        public static void CreateFileWithData(string path, string data)
        {
            if (!File.Exists(path))
            {
                File.Create(path).Dispose();
            }

            using (TextWriter tw = new StreamWriter(path))
            {
                tw.Write(data);
            }
        }
    }
}
