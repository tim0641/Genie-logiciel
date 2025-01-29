using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using EasyLib.Models;

namespace EasyLib.ViewModels
{
    public class BackupViewModel
    {
        private Dictionary<string, BackupModel> backups = new Dictionary<string, BackupModel>();
        private const string BackupFilePath = "backups.json";
        public string Status { get; private set; }

        public BackupViewModel()
        {
            LoadBackups();
        }

        public void CreateBackup(string name, string srcPath, string destPath, string type)
        {
            if (backups.ContainsKey(name))
            {
                Status = "Backup job already exists.";
                return;
            }

            var backup = new BackupModel(name, srcPath, destPath, type, DateTime.Now);
            backups[name] = backup;
            SaveBackups();
            Status = "Backup job created successfully.";
        }

        public void ListBackups()
        {
            if (backups.Count == 0)
            {
                Status = "No backup jobs available.";
                return;
            }

            Status = "List of backup jobs:\n";
            foreach (var backup in backups)
            {
                Status += $"{backup.Key} - {backup.Value.SourcePath} -> {backup.Value.FullDestinationPath} ({backup.Value.BackupType})\n";
            }
        }

        public void RunBackup(string name)
        {
            if (!backups.ContainsKey(name))
            {
                Status = "Backup job not found.";
                return;
            }

            var backup = backups[name];
            try
            {
                if (backup.IsDirectory)
                {
                    CopyDirectory(backup.SourcePath, backup.DestinationPath);
                }
                else
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(backup.FullDestinationPath));
                    File.Copy(backup.SourcePath, backup.FullDestinationPath, true);
                }
                Status = $"Backup {name} completed successfully.";
            }
            catch (Exception ex)
            {
                Status = $"Error during backup: {ex.Message}";
            }
        }

private void CopyDirectory(string sourceDir, string destDir)
{
    // Create the destination directory itself, including the source directory name
    var destDirWithSource = Path.Combine(destDir, Path.GetFileName(sourceDir));
    Directory.CreateDirectory(destDirWithSource);

    foreach (var dir in Directory.GetDirectories(sourceDir, "*", SearchOption.AllDirectories))
    {
        Directory.CreateDirectory(dir.Replace(sourceDir, destDirWithSource));
    }

    foreach (var file in Directory.GetFiles(sourceDir, "*.*", SearchOption.AllDirectories))
    {
        File.Copy(file, file.Replace(sourceDir, destDirWithSource), true);
    }
}
        public void DeleteBackup(string name)
        {
            if (!backups.ContainsKey(name))
            {
                Status = "Backup job not found.";
                return;
            }

            var backup = backups[name];
            backups.Remove(name);
            SaveBackups();

            try
            {
                if (backup.IsDirectory)
                {
                    if (Directory.Exists(backup.DestinationPath))
                    {
                        Directory.Delete(backup.DestinationPath, true);
                        Status = "Backup job and associated directory deleted successfully.";
                    }
                    else
                    {
                        Status = "Backup job deleted, but the associated directory was not found.";
                    }
                }
                else if (File.Exists(backup.FullDestinationPath))
                {
                    File.Delete(backup.FullDestinationPath);
                    Status = "Backup job and associated file deleted successfully.";
                }
                else
                {
                    Status = "Backup job deleted, but the associated file was not found.";
                }
            }
            catch (Exception ex)
            {
                Status = $"Error deleting backup file: {ex.Message}";
            }
        }

        private void SaveBackups()
        {
            var json = JsonSerializer.Serialize(backups);
            File.WriteAllText(BackupFilePath, json);
        }

        private void LoadBackups()
        {
            if (File.Exists(BackupFilePath))
            {
                var json = File.ReadAllText(BackupFilePath);
                backups = JsonSerializer.Deserialize<Dictionary<string, BackupModel>>(json);
            }
        }

        public List<BackupModel> GetBackupList()
        {
            return new List<BackupModel>(backups.Values);
        }
    }
}