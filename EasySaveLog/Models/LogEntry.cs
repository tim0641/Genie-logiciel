namespace EasySaveLog.Models
{
    public class LogEntry
    {
        public DateTime Timestamp { get; set; }
        public string BackupName { get; set; }
        public string SourcePath { get; set; }
        public string DestinationPath { get; set; }
        public long FileSize { get; set; }
        public double? TransferTime { get; set; } // optionnel pour le create
    }
}