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

namespace EasyLib.ViewModels
{
    public class BackupViewModel : INotifyPropertyChanged
    {
        private readonly BackupService _backupService;
        private readonly DailyLogService _dailyLogService;
        private readonly StateService _stateService;

        public ObservableCollection<BackupModel> Backups { get; private set; }

        private string _status;
        public string Status
        {
            get => _status;
            private set
            {
                _status = value;
                OnPropertyChanged();
            }
        }
        

        private string _backupName;
        public string BackupName
        {
            get => _backupName;
            set
            {
                _backupName = value;
                OnPropertyChanged();
            }
        }

        private string _sourcePath;
        public string SourcePath
        {
            get => _sourcePath;
            set
            {
                _sourcePath = value;
                OnPropertyChanged();
            }
        }

        private string _destinationPath;
        public string DestinationPath
        {
            get => _destinationPath;
            set
            {
                _destinationPath = value;
                OnPropertyChanged();
            }
        }

        private string _backupType;
        public string BackupType
        {
            get => _backupType;
            set
            {
            if (_backupType != value)
            {
                _backupType = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsFullBackup)); // Met à jour les CheckBox
                OnPropertyChanged(nameof(IsDifferentialBackup));
            }
            }
        }
        public bool IsFullBackup
        {
            get => BackupType == "Full";
            set
            {
                if (value)
                    BackupType = "Full"; // Si on coche Full, on met à jour BackupType
            }
        }

        public bool IsDifferentialBackup
        {
            get => BackupType == "Differential";
            set
            {
                if (value)
                    BackupType = "Differential"; // Si on coche Differential, on met à jour BackupType
            }
        }
        private bool _isSelectionMode;
        public bool IsSelectionMode
        {
            get => _isSelectionMode;
            set
            {
                _isSelectionMode = value;
                OnPropertyChanged();
            }
        }

     private bool _isEncrypted;
        public bool IsEncrypted
        {
            get => _isEncrypted;
            set
        {
        _isEncrypted = value;
        OnPropertyChanged();
        }
        }   

    private bool _isDecrypted;
    public bool IsDecrypted
{
    get => _isDecrypted;
    set
    {
        _isDecrypted = value;
        OnPropertyChanged();
    }
}


    private string _progressText;
    public string ProgressText
    {
        get => _progressText;
        set
        {
            if (_progressText != value)
            {
                _progressText = value;
                OnPropertyChanged(nameof(ProgressText));
            }
        }}

    private bool _Boolrun;
    public bool Boolrun
    {
        get => _Boolrun;
        set
        {
            if (_Boolrun != value)
            {
                _Boolrun = value;
                OnPropertyChanged(nameof(Boolrun));
                OnBoolrunUpdated(_Boolrun); // Déclenche l'événement
                _backupService.UpdateBoolrunState(_Boolrun);
            }
        }
    }





        public ICommand CreateBackupCommand { get; }
        public ICommand ListBackupsCommand { get; }
        public ICommand RunSelectedBackupCommand { get; }
        public ICommand DeleteSelectedBackupCommand { get; }

        public ICommand StartCommand { get; }
        public ICommand StopCommand { get; }


        public StateService StateService => _stateService;  


        public BackupViewModel(DailyLogService dailyLogService, BackupService backupService, StateService stateService)
        {
            _dailyLogService = dailyLogService;
            _backupService = backupService;
            _stateService = stateService;
            backupService.ProgressUpdated += Service_ProgressUpdated;
            backupService.BoolrunUpdated += OnBoolrunUpdated; // Abonnez le service à l'événement


            Backups = new ObservableCollection<BackupModel>();

            CreateBackupCommand = new RelayCommand(CreateBackupFromUserInput);
            ListBackupsCommand = new RelayCommand(ListBackups);
            RunSelectedBackupCommand = new RelayCommand(RunSelectedBackups);
            DeleteSelectedBackupCommand = new RelayCommand(DeleteSelectedBackups);

            StartCommand = new RelayCommand(() => Boolrun = true);
            StopCommand = new RelayCommand(() => Boolrun = false);
        }






        private void Service_ProgressUpdated(long progress)
        {
            ProgressText = progress.ToString();
        }

        public event Action<bool> BoolrunUpdated;
        private void OnBoolrunUpdated(bool newValue)
        {
            BoolrunUpdated?.Invoke(newValue);
        }


        public void SetBoolrunTrue()
        {
            Boolrun = true;
        }

        public void SetBoolrunFalse()
        {
            Boolrun = false;
        }



        private void CreateBackupFromUserInput()
        {


            if (string.IsNullOrWhiteSpace(BackupName) || string.IsNullOrWhiteSpace(SourcePath) ||
                string.IsNullOrWhiteSpace(DestinationPath) || string.IsNullOrWhiteSpace(BackupType))
            {
                Status = Localization.Get("fill_all_fields");
                return;
            }

            Status = _backupService.CreateBackup(BackupName, SourcePath, DestinationPath, BackupType);
        }



        private void ListBackups()
        {


            Backups.Clear();
            foreach (var backup in _backupService.GetAllBackups())
            {
                Backups.Add(backup);
            }
            Status = Backups.Count == 0 ? Localization.Get("no_backups_found"): "";
        }




private async void RunSelectedBackups()
{
    try
    {
        var selectedBackups = Backups.Where(b => b.IsSelected).Select(b => b.Name).ToList();

        if (selectedBackups.Count == 0)
        {
            Status = Localization.Get("no_backups_selected_for_execution");
            return;
        }

        bool isEncrypted = IsEncrypted; 
        bool isDecrypted = IsDecrypted; 

        Status = Localization.Get("backups_execution_in_progress");
        StateService.StopTimer();

    
        await Task.Run(() => 
        {
            _backupService.RunBackup(selectedBackups, isEncrypted, isDecrypted);
        });

        Status = Localization.Get("execution_completed");
    }
    catch (Exception ex)
    {
        Status = Localization.Get("execution_error") + ": " + ex.Message;
    }
}

        private void DeleteSelectedBackups()
        {

            try
            {
                var selectedBackups = Backups.Where(b => b.IsSelected).Select(b => b.Name).ToList();

                if (selectedBackups.Count == 0)
                {
                    Status = Localization.Get("no_backups_selected_for_deletion");
                    return;
                }

                Status = Localization.Get("backups_deletion_in_progress");
                Status = _backupService.DeleteBackup(selectedBackups);
            }
            catch (Exception ex)
            {
            Status = Localization.Get("deletion_error") + ": " + ex.Message;                  
            }
        }






        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

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


    
}