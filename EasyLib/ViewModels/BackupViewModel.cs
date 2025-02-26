using System;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using EasyLib.Models;
using EasyLib.Services;
using EasySaveLog.Services;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualBasic;
using EasySaveLog.Models;

namespace EasyLib.ViewModels
{
    public class BackupViewModel : INotifyPropertyChanged
    {
        private readonly BackupService _backupService; // Service handling backup operations
        private readonly DailyLogService _dailyLogService; // Service managing daily logs
        private readonly StateService _stateService; // Service handling state tracking

        public ObservableCollection<BackupModel> Backups { get; private set; } // Collection of backups
        public ObservableCollection<EncoursModel> EncoursBackups { get; private set; } = new ObservableCollection<EncoursModel>(); // Collection of ongoing backups

        // Priority model configuration for specific file types
        public PriorityModel Priority { get; set; } = new PriorityModel
        {
            IsDocx = false,
            IsPdf = false,
            IsTxt = false,
            IsJpg = false
        };
        private string _status; // Stores the current status message
        public string Status
        {
            get => _status;
            private set
            {
                _status = value;
                OnPropertyChanged();
            }
        }
        
        private string _backupName; // Stores the backup name
        public string BackupName
        {
            get => _backupName;
            set
            {
                _backupName = value;
                OnPropertyChanged();
            }
        }

        private string _sourcePath; // Stores the source path of the backup
        public string SourcePath
        {
            get => _sourcePath;
            set
            {
                _sourcePath = value;
                OnPropertyChanged();
            }
        }

        private string _destinationPath; // Stores the destination path of the backup
        public string DestinationPath
        {
            get => _destinationPath;
            set
            {
                _destinationPath = value;
                OnPropertyChanged();
            }
        }

        private string _backupType; // Stores the type of backup (Full/Differential)
        public string BackupType
        {
            get => _backupType;
            set
            {
                if (_backupType != value)
                {
                    _backupType = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsFullBackup)); // Updates CheckBox status
                    OnPropertyChanged(nameof(IsDifferentialBackup));
                }
            }
        }

        // Property to check if Full Backup is selected
        public bool IsFullBackup
        {
            get => BackupType == "Full";
            set
            {
                if (value)
                    BackupType = "Full"; // If checked, update BackupType
            }
        }

        // Property to check if Differential Backup is selected
        public bool IsDifferentialBackup
        {
            get => BackupType == "Differential";
            set
            {
                if (value)
                    BackupType = "Differential"; // If checked, update BackupType
            }
        }

        private bool _isEncrypted; // Stores encryption status
        public bool IsEncrypted
        {
            get => _isEncrypted;
            set
            {
                _isEncrypted = value;
                OnPropertyChanged();
            }
        }   

        private bool _isDecrypted; // Stores decryption status
        public bool IsDecrypted
        {
            get => _isDecrypted;
            set
            {
                _isDecrypted = value;
                OnPropertyChanged();
            }
        }

        public LogFormat LogFormat
        {
            get => _dailyLogService.Format;
            set
            {
                _dailyLogService.Format = value; // Updates log format
                OnPropertyChanged();
            }
        }

        private readonly string _processToMonitor = "CalculatorApp"; // The process to monitor
        private bool _isProcessRunning = false; // Tracks if the monitored process is running
        private CancellationTokenSource _monitoringCancellationToken; // Token to cancel monitoring

        // Commands for UI interactions
        public ICommand CreateBackupCommand { get; }
        public ICommand ListBackupsCommand { get; }
        public ICommand RunSelectedBackupCommand { get; }
        public ICommand DeleteSelectedBackupCommand { get; }
        public ICommand CancelCommand { get; }
        public StateService StateService => _stateService;  
        public ICommand PlayCommand { get; }
        public ICommand StopCommand { get; }
        public ICommand DeleteProgressCommand { get; }  

        // Constructor initializing services and commands
        public BackupViewModel(DailyLogService dailyLogService, BackupService backupService, StateService stateService)
        {
            _dailyLogService = dailyLogService;
            _backupService = backupService;
            _stateService = stateService;

            Backups = new ObservableCollection<BackupModel>();
            Application.Current.Dispatcher.InvokeAsync(() => ListBackups());

            // Initializing commands
            CreateBackupCommand = new RelayCommand(CreateBackupFromUserInput);
            ListBackupsCommand = new RelayCommand(ListBackups);
            RunSelectedBackupCommand = new RelayCommand(RunSelectedBackups);
            DeleteSelectedBackupCommand = new RelayCommand(DeleteSelectedBackups);

            EncoursBackups = new ObservableCollection<EncoursModel>();
            PlayCommand = new RelayCommand<int>(PlayBackup);
            StopCommand = new RelayCommand<int>(StopBackup);
            CancelCommand = new RelayCommand<int>(CancelBackup);
            DeleteProgressCommand = new RelayCommand<int>(DeleteProgress);

            // Clearing state file at initialization
            File.WriteAllText("C:\\Logs\\States\\Daily\\state.json", string.Empty);
        }
    

// Text properties for UI localization
public string PlayText => Localization.Get("start"); // Gets the localized text for "start"
public string StopText => Localization.Get("stop"); // Gets the localized text for "stop"
public string CancelText => Localization.Get("cancel"); // Gets the localized text for "cancel"
public string DelText => Localization.Get("del"); // Gets the localized text for "delete"

// Refreshes the localized text for UI elements
public void RefreshLocalization()
{
    OnPropertyChanged(nameof(PlayText));
    OnPropertyChanged(nameof(StopText));
    OnPropertyChanged(nameof(CancelText));
    OnPropertyChanged(nameof(DelText));
    
    // Refresh the state of all ongoing backups
    foreach (var item in EncoursBackups)
    {
        item.RefreshEtat();
    }
}

// Starts or resumes a backup process
private void PlayBackup(int backupId)
{
    var encours = EncoursBackups.FirstOrDefault(e => e.ID == backupId); // Finds the ongoing backup
    var backup = Backups.FirstOrDefault(b => b.ID == backupId); // Finds the corresponding backup

    if (encours != null && backup != null)
    {
        if (encours.Cancelled)
        {
            // If the backup was canceled, reset and restart it
            encours.Cancelled = false;
            encours.Progress = 0;
            Task.Run(() =>
            {
                _backupService.RunBackup(backup, encours, Priority, IsEncrypted, IsDecrypted);
            });
        }
        else
        {
            // If it's not canceled, resume the backup process
            encours.EnCoursbool = true;
        }
    }
}

// Stops a running backup process
private void StopBackup(int backupId)
{
    var backup = EncoursBackups.FirstOrDefault(e => e.ID == backupId);
    if (backup != null)
    {
        backup.EnCoursbool = false; // Updates the "In Progress" status to false
    }
}

// Deletes progress tracking of a backup process
private void DeleteProgress(int backupId)
{
    var progressItem = EncoursBackups.FirstOrDefault(e => e.ID == backupId);
    if (progressItem != null)
    {
        // Reset the progress and remove it from the ongoing backups list
        progressItem.Progress = 0;
        progressItem.EnCoursbool = false;
        EncoursBackups.Remove(progressItem);
    }

    // Find the corresponding backup and unselect it
    var backupItem = Backups.FirstOrDefault(b => b.ID == backupId);
    if (backupItem != null)
    {
        backupItem.IsSelected = false;
        _stateService.StopStateForBackup(backupItem.Name); // Remove state tracking
    }
}

// Cancels an ongoing backup process
private void CancelBackup(int backupId)
{
    var encours = EncoursBackups.FirstOrDefault(e => e.ID == backupId);
    var backup = Backups.FirstOrDefault(b => b.ID == backupId);

    if (encours != null && backup != null)
    {
        encours.Cancelled = true; // Marks the backup as canceled
        encours.EnCoursbool = false;
        encours.Progress = 0;
        backup.IsSelected = false;
        _backupService.CancelBackup(backup); // Calls the service to cancel the backup
    }
}

// Starts monitoring for a specific process (e.g., "CalculatorApp") and pauses backups if detected
public void StartProcessMonitoring()
{
    _monitoringCancellationToken = new CancellationTokenSource();

    Task.Run(() =>
    {
        while (!_monitoringCancellationToken.Token.IsCancellationRequested)
        {
            // Checks if the process (CalculatorApp or "calc") is running
            bool processDetected = Process.GetProcessesByName(_processToMonitor).Length > 0 ||
                                   Process.GetProcessesByName("calc").Length > 0;

            if (processDetected && !_isProcessRunning)
            {
                _isProcessRunning = true;
                PauseAllBackups(); // Pauses all backups when the process is detected
            }
            else if (!processDetected && _isProcessRunning)
            {
                _isProcessRunning = false;
                ResumeAllBackups(); // Resumes backups when the process is no longer running
            }

            Thread.Sleep(5000); // Checks the process status every 5 seconds
        }
    }, _monitoringCancellationToken.Token);
}

// Pauses all active backups
private void PauseAllBackups()
{
    foreach (var encours in EncoursBackups)
    {
        encours.EnCoursbool = false; // Updates all backups to paused state
    }
    Status = "Backups paused (Business software in use)";
}

// Resumes all paused backups
private void ResumeAllBackups()
{
    foreach (var encours in EncoursBackups)
    {
        encours.EnCoursbool = true; // Updates all backups to running state
    }
    Status = "Backups resumed (Calculator closed)";
}

// Stops the process monitoring
public void StopProcessMonitoring()
{
    _monitoringCancellationToken?.Cancel(); // Cancels the monitoring task
}
// Creates a backup from user input
private void CreateBackupFromUserInput()
{
    // Check if all required fields are filled
    if (string.IsNullOrWhiteSpace(BackupName) || string.IsNullOrWhiteSpace(SourcePath) ||
        string.IsNullOrWhiteSpace(DestinationPath) || string.IsNullOrWhiteSpace(BackupType))
    {
        Status = Localization.Get("fill_all_fields"); // Notify user to fill all fields
        return;
    }

    // Call the backup service to create the backup
    Status = _backupService.CreateBackup(BackupName, SourcePath, DestinationPath, BackupType);
}

// Retrieves and lists all existing backups
private void ListBackups()
{
    Backups.Clear(); // Clear the current backup list before refreshing

    foreach (var backup in _backupService.GetAllBackups())
    {
        // If it's a directory, count the number of files, otherwise set to 1
        backup.FileCount = backup.IsDirectory 
            ? _backupService.CountFilesInDirectory(backup.SourcePath) 
            : 1;

        // Calculate the total size of the backup
        backup.TotalSize = _backupService.GetSize(backup.SourcePath, backup.DestinationPath, backup.BackupType);

        // Add the backup to the collection
        Backups.Add(backup);
    }

    // Update the status message
    Status = Backups.Count == 0 ? Localization.Get("no_backups_found") : "";
}

// Runs the selected backups asynchronously
private async void RunSelectedBackups()
{
    try
    {
        var selectedBackups = Backups.Where(b => b.IsSelected).Select(b => b.ID).ToList();

        bool isEncrypted = IsEncrypted;
        bool isDecrypted = IsDecrypted;

        Status = Localization.Get("backups_execution_in_progress"); // Update UI status
        StateService.StopTimer(); // Stop any active timers

        var selectedBackupsList = Backups.Where(b => b.IsSelected).ToList();

        // Add selected backups to the ongoing backup list
        foreach (var backup in selectedBackupsList)
        {
            if (!EncoursBackups.Any(e => e.ID == backup.ID))
            {
                var encours = new EncoursModel(backup.ID, backup.Name, backup.BackupType);
                EncoursBackups.Add(encours);
            }
        }

        List<Task> tasks = new List<Task>();

        // Start each selected backup asynchronously
        foreach (var backupId in selectedBackups)
        {
            var backup = Backups.FirstOrDefault(b => b.ID == backupId);
            var encours = EncoursBackups.FirstOrDefault(e => e.ID == backupId);

            if (backup != null && encours != null)
            {
                tasks.Add(Task.Run(() =>
                {
                    _backupService.RunBackup(backup, encours, Priority, isEncrypted, isDecrypted);
                }));
            }
        }

        // Wait for all backups to complete
        await Task.WhenAll(tasks);
        Status = Localization.Get("execution_completed"); // Update UI status
    }
    catch (Exception ex)
    {
        Status = Localization.Get("execution_error") + ": " + ex.Message; // Handle errors
    }
}

// Deletes the selected backups
private void DeleteSelectedBackups()
{
    try
    {
        var selectedBackups = Backups.Where(b => b.IsSelected).Select(b => b.Name).ToList();

        // If no backups are selected, notify the user
        if (selectedBackups.Count == 0)
        {
            Status = Localization.Get("no_backups_selected_for_deletion");
            return;
        }

        Status = Localization.Get("backups_deletion_in_progress"); // Update UI status

        // Call the backup service to delete the backups
        Status = _backupService.DeleteBackup(selectedBackups);
    }
    catch (Exception ex)
    {
        Status = Localization.Get("deletion_error") + ": " + ex.Message; // Handle errors
    }
}

// Event handler for property change notifications
public event PropertyChangedEventHandler? PropertyChanged;
protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
{
    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}

// Relay command implementation for UI command binding
public class RelayCommand : ICommand
{
    private readonly Action _execute;
    private readonly Func<bool> _canExecute;
    public event EventHandler? CanExecuteChanged;

    public RelayCommand(Action execute, Func<bool> canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public bool CanExecute(object parameter) => _canExecute == null || _canExecute();
    public void Execute(object parameter) => _execute();
}

// Generic relay command to handle commands with parameters
public class RelayCommand<T> : ICommand
{
    private readonly Action<T> _execute;
    private readonly Func<T, bool> _canExecute;
    public event EventHandler? CanExecuteChanged;

    public RelayCommand(Action<T> execute, Func<T, bool> canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public bool CanExecute(object parameter) => _canExecute == null || _canExecute((T)parameter);
    public void Execute(object parameter) => _execute((T)parameter);
}}}