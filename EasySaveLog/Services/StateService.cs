using System;
using System.IO;
using System.Threading;
using EasySaveLog.Models;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace EasySaveLog.Services
{
    public class StateService
    {
        private readonly string _logDirectory;
        private Timer? _timer;  // Timer pour la mise à jour continue
        private bool _isTimerActive = false;  // Indicateur si le timer est actif

        // Utilisation d'un verrou et d'un dictionnaire pour stocker les états actuels,
        // indexés par le nom du backup (ou une autre clé unique)
        private static readonly object _fileLock = new object();
        private static readonly Dictionary<string, string> _currentStates = new Dictionary<string, string>();

        public StateService(string logDirectory)
        {
            _logDirectory = logDirectory;
            Directory.CreateDirectory(_logDirectory);
            var logFilePath = Path.Combine(_logDirectory, "states.log");
            File.WriteAllText(logFilePath, "");

        }
        

        public void TakeAndUpdateStates(string? name, string? sourceFilePath, string? targetFilePath, string state, string? type, long? totalFilesToCopy, long? totalFilesSize, long? nbFilesLeftToDo, long? progression)
        {
            var stateEntry = new StateEntry
            {
                Name = name,
                LastActionTimestamp = DateTime.Now,
                SourceFilePath = sourceFilePath,
                TargetFilePath = targetFilePath,
                State = state,
                Type = type,
                TotalFilesToCopy = totalFilesToCopy,
                TotalFilesSize = totalFilesSize,
                NbFilesLeftToDo = nbFilesLeftToDo,
                Progression = progression
            };

            string formattedEntry = $@"
{{
    Name = {stateEntry.Name},
    LastActionTimestamp = {stateEntry.LastActionTimestamp:yyyy-MM-dd HH:mm:ss},
    SourceFilePath = {stateEntry.SourceFilePath},
    TargetFilePath = {stateEntry.TargetFilePath},
    State = {stateEntry.State},
    Type = {stateEntry.Type},
    TotalFilesToCopy = {stateEntry.TotalFilesToCopy},
    TotalFilesSize = {stateEntry.TotalFilesSize},
    NbFilesLeftToDo = {stateEntry.NbFilesLeftToDo},
    Progression = {stateEntry.Progression} %
}}";

            lock (_fileLock)
            {
                if (!string.IsNullOrEmpty(name))
                {
                    // Mettre à jour ou ajouter l'entrée correspondant au backup
                    _currentStates[name] = formattedEntry;
                }
                else
                {
                    // Si le nom est null ou vide, utiliser une clé temporaire
                    _currentStates[Guid.NewGuid().ToString()] = formattedEntry;
                }

                // Réécrire entièrement le fichier avec l'ensemble des états actuels
                var logFilePath = Path.Combine(_logDirectory, "states.log");
                File.WriteAllText(logFilePath, string.Join(Environment.NewLine, _currentStates.Values));
            }
        }

        // Méthode pour supprimer un état lorsqu'un backup est terminé ou annulé
        public void StopStateForBackup(string name)
        {
            lock (_fileLock)
            {
                if (_currentStates.ContainsKey(name))
                {
                    _currentStates.Remove(name);
                    var logFilePath = Path.Combine(_logDirectory, "states.log");
                    File.WriteAllText(logFilePath, string.Join(Environment.NewLine, _currentStates.Values));
                }
            }
        }

        public void StartTimer(string? name, string? sourceFilePath, string? targetFilePath, string state, string? type, long? totalFilesToCopy, long? totalFilesSize, long? nbFilesLeftToDo, long? progression)
        {
            if (_isTimerActive) return;  // Ne pas démarrer si déjà actif

            _timer = new Timer((e) =>
            {
                TakeAndUpdateStates(name, sourceFilePath, targetFilePath, state, type, totalFilesToCopy, totalFilesSize, nbFilesLeftToDo, progression);
            }, null, 0, 1000);  // Mise à jour toutes les secondes
            _isTimerActive = true;
        }

        public void StopTimer()
        {
            _timer?.Change(Timeout.Infinite, 0);  // Arrête le timer
            _isTimerActive = false;
        }
    }
}
