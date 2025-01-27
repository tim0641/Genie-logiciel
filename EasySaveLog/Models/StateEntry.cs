namespace EasySaveLog.Models
{
    public class StateEntry
    {
        public string BackupName { get; set; }
        public DateTime LastActionTimestamp { get; set; }
        public string Status { get; set; } 
        public int TotalFiles { get; set; }
        public long TotalSize { get; set; }
        public int Progress { get; set; } 
        public int RemainingFiles { get; set; }
        public long RemainingSize { get; set; }
        public string CurrentSource { get; set; }
        public string CurrentDestination { get; set; }
    }
}