namespace EasySaveLog.Models
{
    public class StateEntry
    {
        public string? Name { get; set; }
        public DateTime LastActionTimestamp { get; set; }
        public string? SourceFilePath { get; set; } 
        public string? TargetFilePath { get; set; } 
        public string? State { get; set; } 
        public string? Type { get; set; } 
        public long? TotalFilesToCopy { get; set; }
        public long? TotalFilesSize { get; set; } 
        public long? NbFilesLeftToDo { get; set; }
        public long? Progression { get; set; }


    }
}