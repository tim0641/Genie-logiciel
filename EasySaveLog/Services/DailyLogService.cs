using System.Text.Json;
using EasySaveLog.Models;

namespace EasySaveLog.Services
{
    public class DailyLogService
    {
        private readonly string _logsDirectory;

        public DailyLogService(string logsDirectory)
        {
            _logsDirectory = logsDirectory;
            Directory.CreateDirectory(_logsDirectory);
        }

        public void WriteLogEntry(LogEntry entry)
        {
            
            string fileName = DateTime.Now.ToString("yyyy-MM-dd") + ".json";
            string filePath = Path.Combine(_logsDirectory, fileName);

           
            string json = JsonSerializer.Serialize(entry, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            
            File.AppendAllText(filePath, json + Environment.NewLine);
        }
        public long GetFileSize(string filePath)
        {
            if (File.Exists(filePath))
            {
                FileInfo fileInfo = new FileInfo(filePath);
                return fileInfo.Length;
            }
            return 0;
        }
    }
    
}


