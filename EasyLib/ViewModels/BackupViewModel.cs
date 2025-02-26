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
        private readonly BackupService _backupService;
        private readonly DailyLogService _dailyLogService;
        private readonly StateService _stateService;

        public ObservableCollection<BackupModel> Backups { get; private set; }
    public ObservableCollection<EncoursModel> EncoursBackups { get; private set; } = new ObservableCollection<EncoursModel>();

    public PriorityModel Priority { get; set; } = new PriorityModel
    {
        IsDocx = false,
        IsPdf = false,
        IsTxt = false,
        IsJpg = false
    };

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

    public LogFormat LogFormat
{
    get => _dailyLogService.Format;
    set
    {
        _dailyLogService.Format = value; // Met à jour le format du service de logs
        OnPropertyChanged();
    }
}

private readonly string _processToMonitor = "CalculatorApp"; // Processus à surveiller
private bool _isProcessRunning = false;
private CancellationTokenSource _monitoringCancellationToken; // Annuler le jeton pour arrêter la surveillance

        public ICommand CreateBackupCommand { get; }
        public ICommand ListBackupsCommand { get; }
        public ICommand RunSelectedBackupCommand { get; }
        public ICommand DeleteSelectedBackupCommand { get; }
        public ICommand CancelCommand { get; }
        public StateService StateService => _stateService;  
        public ICommand PlayCommand { get; }
        public ICommand StopCommand { get; }
        public ICommand DeleteProgressCommand { get; }  
        public BackupViewModel(DailyLogService dailyLogService, BackupService backupService, StateService stateService)
        {
            _dailyLogService = dailyLogService;
            _backupService = backupService;
            _stateService = stateService;

            Backups = new ObservableCollection<BackupModel>();
            Application.Current.Dispatcher.InvokeAsync(() => ListBackups());
            
            CreateBackupCommand = new RelayCommand(CreateBackupFromUserInput);
            ListBackupsCommand = new RelayCommand(ListBackups);
            RunSelectedBackupCommand = new RelayCommand(RunSelectedBackups);
            DeleteSelectedBackupCommand = new RelayCommand(DeleteSelectedBackups);


            EncoursBackups = new ObservableCollection<EncoursModel>();
            PlayCommand = new RelayCommand<int>(PlayBackup);
            StopCommand = new RelayCommand<int>(StopBackup);
            CancelCommand = new RelayCommand<int>(CancelBackup);
            DeleteProgressCommand = new RelayCommand<int>(DeleteProgress); 
            
        string stateFilePath = Path.Combine(PathHelper.GetStatesDirectory(), "state.json");
        File.WriteAllText(stateFilePath, string.Empty);

        }

public string PlayText => Localization.Get("start");
public string StopText => Localization.Get("stop");
public string CancelText => Localization.Get("cancel");
public string DelText => Localization.Get("del");

public void RefreshLocalization()
{
    OnPropertyChanged(nameof(PlayText));
    OnPropertyChanged(nameof(StopText));
    OnPropertyChanged(nameof(CancelText));
    OnPropertyChanged(nameof(DelText));
        foreach (var item in EncoursBackups)
    {
        item.RefreshEtat();
        
    }
}





private void PlayBackup(int backupId)
{
    var encours = EncoursBackups.FirstOrDefault(e => e.ID == backupId);
    var backup = Backups.FirstOrDefault(b => b.ID == backupId);
    
    if (encours != null && backup != null)
    {
        if (encours.Cancelled)
        {
            encours.Cancelled = false;
            encours.Progress = 0;
            Task.Run(() =>
            {
                _backupService.RunBackup(backup, encours,Priority, IsEncrypted, IsDecrypted);
            });
        }
        else
        {
            encours.EnCoursbool = true;
        }
    }
}

private void StopBackup(int backupId)
{

    var backup = EncoursBackups.FirstOrDefault(e => e.ID == backupId);
    if (backup != null)
    {
        backup.EnCoursbool = false; // Mettre à jour l'attribut "EnCours" de la backup à "false"
    }
}
private void DeleteProgress(int backupId)
{
    // Retrouver l'élément de progression dans la collection d'états en cours
    var progressItem = EncoursBackups.FirstOrDefault(e => e.ID == backupId);
    if (progressItem != null)
    {
        // Réinitialise la progression et l'état de l'élément
        progressItem.Progress = 0;
        progressItem.EnCoursbool = false;
        // Supprime l'élément de la collection pour qu'il disparaisse du DataGrid
        EncoursBackups.Remove(progressItem);
    }

    // Retrouver le backup dans la collection principale et décocher sa sélection
    var backupItem = Backups.FirstOrDefault(b => b.ID == backupId);
    if (backupItem != null)
    {
        backupItem.IsSelected = false;
        // Appeler le StateService pour supprimer l'état correspondant dans le fichier d'états
        _stateService.StopStateForBackup(backupItem.Name);
    }
}


private void CancelBackup(int backupId)
{
    var encours = EncoursBackups.FirstOrDefault(e => e.ID == backupId);
    var backup = Backups.FirstOrDefault(b => b.ID == backupId);
    if (encours != null && backup != null)
    {
        encours.Cancelled = true;
        encours.EnCoursbool = false;
        encours.Progress = 0;
        backup.IsSelected = false;
        _backupService.CancelBackup(backup);
    }
}



public void StartProcessMonitoring()
{
    _monitoringCancellationToken = new CancellationTokenSource();

    Task.Run(() =>
    {
        while (!_monitoringCancellationToken.Token.IsCancellationRequested)
        {
            bool processDetected = Process.GetProcessesByName(_processToMonitor).Length > 0 ||
                                   Process.GetProcessesByName("calc").Length > 0; // Vérifie les 2 versions

            if (processDetected && !_isProcessRunning)
            {
                _isProcessRunning = true;
                PauseAllBackups();
            }
            else if (!processDetected && _isProcessRunning)
            {
                _isProcessRunning = false;
                ResumeAllBackups();
            }

            Thread.Sleep(5000); // Vérifie toutes les 5 secondes
        }
    }, _monitoringCancellationToken.Token);
}




private void PauseAllBackups()
{
    foreach (var encours in EncoursBackups)
    {
        encours.EnCoursbool = false; // Met en pause tous les backups en cours
    }
    Status = "Backups mis en pause (Logiciel métier en cours d'utilisation)";
}




private void ResumeAllBackups()
{
    foreach (var encours in EncoursBackups)
    {
        encours.EnCoursbool = true; // Reprend tous les backups
    }
    Status = "Backups repris (Calculatrice fermée)";
}


public void StopProcessMonitoring()
{
    _monitoringCancellationToken?.Cancel(); // Annuler la surveillance
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

                        backup.FileCount = backup.IsDirectory 
            ? _backupService.CountFilesInDirectory(backup.SourcePath) 
            : 1;
        backup.TotalSize = _backupService.GetSize(backup.SourcePath, backup.DestinationPath, backup.BackupType);
       
                Backups.Add(backup);
            }
            Status = Backups.Count == 0 ? Localization.Get("no_backups_found"): "";
        }




private async void RunSelectedBackups()
{
    try
    {
        var selectedBackups = Backups.Where(b => b.IsSelected).Select(b => b.ID).ToList();   

        bool isEncrypted = IsEncrypted; 
        bool isDecrypted = IsDecrypted; 

        Status = Localization.Get("backups_execution_in_progress");
        StateService.StopTimer();


        var selectedBackupsd = Backups.Where(b => b.IsSelected).ToList();
        foreach (var backup in selectedBackupsd)
        {
            if (!EncoursBackups.Any(e => e.ID == backup.ID))
            {
                var encours = new EncoursModel(backup.ID,  backup.Name, backup.BackupType);
                EncoursBackups.Add(encours);
                
            }
        
        }
        List<Task> tasks = new List<Task>();

        foreach (var backupId in selectedBackups)
        {
            var backup = Backups.FirstOrDefault(b => b.ID == backupId);
            var encours = EncoursBackups.FirstOrDefault(e => e.ID == backupId); 


            if (backup != null && encours != null)
            {
                tasks.Add(Task.Run(() =>
                {
                    _backupService.RunBackup(backup, encours,Priority, isEncrypted, isDecrypted);
                }));
            }
        }       





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
    }


    
}