using System.Text.Json;
using EasySaveLog.Models;

namespace EasySaveLog.Services
{
    public class StateService
    {
        private readonly string _stateFilePath;

        public StateService(string stateFilePath)
        {
            _stateFilePath = stateFilePath;
        }

        public void UpdateState(StateEntry newState)
        {
            
            List<StateEntry> allStates = new List<StateEntry>();
            if (File.Exists(_stateFilePath))
            {
                string existingJson = File.ReadAllText(_stateFilePath);
                allStates = JsonSerializer.Deserialize<List<StateEntry>>(existingJson)
                           ?? new List<StateEntry>();
            }

            
            var existing = allStates.Find(s => s.BackupName == newState.BackupName);
            if (existing == null)
            {
                allStates.Add(newState);
            }
            else
            {
               
                existing.LastActionTimestamp = newState.LastActionTimestamp;
                existing.Status = newState.Status;
                existing.TotalFiles = newState.TotalFiles;
                existing.TotalSize = newState.TotalSize;
                existing.Progress = newState.Progress;
                existing.RemainingFiles = newState.RemainingFiles;
                existing.RemainingSize = newState.RemainingSize;
                existing.CurrentSource = newState.CurrentSource;
                existing.CurrentDestination = newState.CurrentDestination;
            }

            
            string json = JsonSerializer.Serialize(
                allStates,
                new JsonSerializerOptions { WriteIndented = true }
            );
            File.WriteAllText(_stateFilePath, json);
        }
    }
}