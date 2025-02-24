using System;
using System.IO;
using System.ComponentModel;

namespace EasyLib.Models
{
    public class BackupModel : INotifyPropertyChanged
    {
        public string Name { get; }
        public string SourcePath { get; }
        public string DestinationPath { get; }
        public string BackupType { get; }
        public DateTime CreationTime { get; }
        public string FileName { get; }
        public string FullDestinationPath { get; }

        public bool IsEncrypted { get; }
        public bool IsDecrypted { get; }
        public bool IsDirectory { get; } 

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                OnPropertyChanged(nameof(IsSelected));
            }
        }

        private bool _isSelectionMode; 
        public bool IsSelectionMode
        {
            get => _isSelectionMode;
            set
            {
                _isSelectionMode = value;
                OnPropertyChanged(nameof(IsSelectionMode));
            }
        }
    private int _fileCount;
    public int FileCount
    {
        get => _fileCount;
        set
        {
            _fileCount = value;
            OnPropertyChanged(nameof(FileCount));
        }
    }

    private long _totalSize;
    public long TotalSize
    {
        get => _totalSize;
        set
        {
            _totalSize = value;
            OnPropertyChanged(nameof(TotalSize));
        }
    }

private static int _lastId = 0; 
public int ID { get; }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public BackupModel(string name, string sourcePath, string destinationPath, string backupType, DateTime creationTime, bool isEncrypted, bool isDecrypted)
        {
            Name = name;
            SourcePath = sourcePath;
            DestinationPath = destinationPath;
            BackupType = backupType;
            CreationTime = creationTime;
            IsDirectory = Directory.Exists(sourcePath);

            IsEncrypted = isEncrypted;
            IsDecrypted = isDecrypted;
            IsSelectionMode = false; // ✅ Initialisé à false par défaut
            FileCount = 0;
            TotalSize =0;
            FileName = Path.GetFileName(sourcePath);
            FullDestinationPath = Path.Combine(destinationPath, FileName);

            ID = ++_lastId;
            
        }
    

    }
}