using System;
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
                _backupType = value;
                OnPropertyChanged();
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



        public ICommand CreateBackupCommand { get; }
        public ICommand ListBackupsCommand { get; }
        public ICommand RunSelectedBackupCommand { get; }
        public ICommand DeleteSelectedBackupCommand { get; }



        
        public BackupViewModel(DailyLogService dailyLogService, BackupService backupService, StateService stateService)
        {
            _dailyLogService = dailyLogService;
            _backupService = backupService;
            _stateService = stateService;

            Backups = new ObservableCollection<BackupModel>();

            CreateBackupCommand = new RelayCommand(CreateBackupFromUserInput);
            ListBackupsCommand = new RelayCommand(ListBackups);
            RunSelectedBackupCommand = new RelayCommand(RunSelectedBackups);
            DeleteSelectedBackupCommand = new RelayCommand(DeleteSelectedBackups);
        }



        private void CreateBackupFromUserInput()
        {
            if (string.IsNullOrWhiteSpace(BackupName) || string.IsNullOrWhiteSpace(SourcePath) ||
                string.IsNullOrWhiteSpace(DestinationPath) || string.IsNullOrWhiteSpace(BackupType))
            {
                Status = "Veuillez remplir tous les champs.";
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
            Status = Backups.Count == 0 ? "Aucune sauvegarde trouvée." : "Sauvegardes chargées.";
        }




        private async void RunSelectedBackups()
        {
            try
            {
                var selectedBackups = Backups.Where(b => b.IsSelected).Select(b => b.Name).ToList();

                if (selectedBackups.Count == 0)
                {
                    Status = "Aucune sauvegarde sélectionnée pour l'exécution.";
                    return;
                }

                bool isEncrypted = IsEncrypted; 
                bool isDecrypted = IsDecrypted; 

                Status = "Exécution des sauvegardes en cours...";
              
                 Status = $"\nMode sélectionné : {(isEncrypted ? "Chiffrement" : isDecrypted ? "Déchiffrement" : "Aucune action")}";
                 
        Status += $"\nisEncrypted: {isEncrypted}, isDecrypted: {isDecrypted}";
        

                Status = _backupService.RunBackup(selectedBackups, isEncrypted, isDecrypted);
                Status += "\nExécution terminée.";
            }
            catch (Exception ex)
            {
                Status = $"Erreur lors de l'exécution : {ex.Message}";
            }
        }

        private void DeleteSelectedBackups()
        {
            try
            {
                var selectedBackups = Backups.Where(b => b.IsSelected).Select(b => b.Name).ToList();

                if (selectedBackups.Count == 0)
                {
                    Status = "Aucune sauvegarde sélectionnée pour la suppression.";
                    return;
                }

                Status = "Suppression des sauvegardes en cours...";
                Status = _backupService.DeleteBackup(selectedBackups);
            }
            catch (Exception ex)
            {
                Status = $"Erreur lors de la suppression : {ex.Message}";
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