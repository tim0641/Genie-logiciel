using System;
using System.IO;
using System.Text.Json;
using System.Xml.Serialization;
using EasySaveLog.Models;

namespace EasySaveLog.Services
{
    public class DailyLogService
    {
        public LogFormat Format { get; set; } = LogFormat.JSON;
        private readonly string _logsDirectory;
        

        public DailyLogService(string logsDirectory)
        {
            _logsDirectory = logsDirectory;
            Directory.CreateDirectory(_logsDirectory);
        }

        public void WriteLogEntry(LogEntry entry)
        {
            string fileName = DateTime.Now.ToString("yyyy-MM-dd") + (Format == LogFormat.JSON ? ".json" : ".xml");
            string filePath = Path.Combine(_logsDirectory, fileName);

             string logContent = Format == LogFormat.JSON
            ? JsonSerializer.Serialize(entry, new JsonSerializerOptions { WriteIndented = true })
            : SerializeToXml(entry);

            File.AppendAllText(filePath, logContent + Environment.NewLine);
        }

    private string SerializeToXml(LogEntry entry)
    {
        using (var stringWriter = new StringWriter())
        {
            var serializer = new XmlSerializer(typeof(LogEntry));
            serializer.Serialize(stringWriter, entry);
            return stringWriter.ToString();
        }
    }

        public void FlushLogs()
        {
            Console.Out.Flush();
        }
    }
}