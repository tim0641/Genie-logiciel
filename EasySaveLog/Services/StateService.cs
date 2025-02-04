using System;
using System.IO;
using System.Threading;
using System.Security.Cryptography.X509Certificates;
using EasySaveLog.Models;
using Newtonsoft.Json;


namespace EasySaveLog.Services
{

    
    public class StateService
    {
        private readonly string _logDirectory;
        private Timer? _timer;  // Timer pour la mise à jour continue
        private bool _isTimerActive = false;  // Indicateur de si le timer doit être actif


        public StateService(string logDirectory)
        {
            _logDirectory = logDirectory;
            Directory.CreateDirectory(_logDirectory);
        }
        public void TakeAndUpdateStates(string? name,string? sourceFilePath,string? targetFilePath,string state,string? type,long? totalFilesToCopy,long? totalFilesSize,long? nbFilesLeftToDo, long? progression)
        {
            var stateEntry = new StateEntry
            {
                Name=name,
                LastActionTimestamp = DateTime.Now,
                SourceFilePath= sourceFilePath,  
                TargetFilePath=targetFilePath,
                State=state,
                Type=type,
                TotalFilesToCopy= totalFilesToCopy,
                TotalFilesSize= totalFilesSize,
                NbFilesLeftToDo= nbFilesLeftToDo,
                Progression= progression
            };

            string formattedEntry = $@"
                {{
                    Name = {stateEntry.Name},
                    LastActionTimestamp = {stateEntry.LastActionTimestamp:yyyy-MM-dd HH:mm:ss},
                    SourceFilePath = {stateEntry.SourceFilePath},
                    TargetFilePath = {stateEntry.TargetFilePath},
                    State = {stateEntry.State},
                    Type = {stateEntry.Type}
                    TotalFilesToCopy = {stateEntry.TotalFilesToCopy},
                    TotalFilesSize = {stateEntry.TotalFilesSize},
                    NbFilesLeftToDo = {stateEntry.NbFilesLeftToDo},
                    Progression = {stateEntry.Progression} ;
                }}";

                    AppendStates(formattedEntry);
        }

        public void AppendStates(string formattedEntry)
        {
            var logFilePath = Path.Combine(_logDirectory, "states.log");
            File.WriteAllText(logFilePath, formattedEntry);  // Remplacer l'ancien contenu
        }
        public void StartTimer(string? name, string? sourceFilePath, string? targetFilePath, string state, string? type, long? totalFilesToCopy, long? totalFilesSize, long? nbFilesLeftToDo, long? progression)
        {
            if (_isTimerActive) return;  // Ne pas démarrer si le timer est déjà actif.

            _timer = new Timer((e) =>
            {
                TakeAndUpdateStates(name, sourceFilePath, targetFilePath, state, type, totalFilesToCopy, totalFilesSize, nbFilesLeftToDo, progression);
            }, null, 0, 1000);  // Met à jour toutes les secondes
            _isTimerActive = true;
        }

        public void StopTimer()
        {
            _timer?.Change(Timeout.Infinite, 0);  // Arrête le timer
            _isTimerActive = false;
        }
        }

    }