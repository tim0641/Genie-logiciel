using System;
using System.IO;
using System.Text.Json;
using EasySaveLog.Models;

namespace EasySaveLog.Services
{
    // Service pour écrire les logs dans un fichier journalier
    public class DailyLogService
    {
        private readonly string _logsDirectory;

        public DailyLogService(string logsDirectory)
        {
            _logsDirectory = logsDirectory;
            Directory.CreateDirectory(_logsDirectory);
        }
        // Méthode pour écrire une entrée de log dans un fichier journalier
        public void WriteLogEntry(LogEntry entry)
        {
            string fileName = DateTime.Now.ToString("yyyy-MM-dd") + ".json";
            string filePath = Path.Combine(_logsDirectory, fileName);

            string json = JsonSerializer.Serialize(entry, new JsonSerializerOptions { WriteIndented = true });

            File.AppendAllText(filePath, json + Environment.NewLine);
        }

        public void FlushLogs()
        {
            Console.Out.Flush();
        }
    }
}