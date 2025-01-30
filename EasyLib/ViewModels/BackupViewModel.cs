using System;
using System.Collections.Generic;
using System.IO;
using Spectre.Console;
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
                    CopyDirectory(backup.SourcePath, backup.DestinationPath, backup.BackupType);
                }
                else
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(backup.FullDestinationPath));
                     CopyFile(backup.SourcePath, backup.FullDestinationPath, backup.BackupType);
                }
                Status = $"Backup {name} ({backup.BackupType}) completed successfully.";
            }
            catch (Exception ex)
            {
                Status = $"Error during backup: {ex.Message}";
            }
        }

        public void RunAllBackups()
        {
            if (backups.Count == 0)
            {
                Status = "No backups available.";
                return;
            }
            List<String> allStatus = new List<String>();

            Status = "Running all backups...\n";
            foreach (var backup in backups.Values)
            {
                RunBackup(backup.Name);
                allStatus.Add( $"{backup.Name} - {Status}");
            }
            Status = string.Join("\n", allStatus);
        }

private void CopyDirectory(string sourceDir, string destDir, string backupType)
{
    var destDirWithSource = Path.Combine(destDir, Path.GetFileName(sourceDir));
    Directory.CreateDirectory(destDirWithSource);

    foreach (var dir in Directory.GetDirectories(sourceDir, "*", SearchOption.AllDirectories))
    {
        Directory.CreateDirectory(dir.Replace(sourceDir, destDirWithSource));
    }

    foreach (var file in Directory.GetFiles(sourceDir, "*.*", SearchOption.AllDirectories))
    {
        string destinationFilePath = file.Replace(sourceDir, destDirWithSource);
        CopyFile(file, destinationFilePath, backupType);
    }
}

private void CopyFile(string sourceFile, string destFile, string backupType)
{
    if (backupType.ToLower() == "full" || !File.Exists(destFile))
    {
        File.Copy(sourceFile, destFile, true);
    }
    else
    {
        var sourceInfo = new FileInfo(sourceFile);
        var destInfo = new FileInfo(destFile);

        
        if (sourceInfo.LastWriteTime > destInfo.LastWriteTime || sourceInfo.Length != destInfo.Length)
        {
            File.Copy(sourceFile, destFile, true);
        }
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
                    if (Directory.Exists(backup.FullDestinationPath))
                    {
                        Directory.Delete(backup.FullDestinationPath, true);
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
        public BackupModel ShowBackup(string title)
        {
            AnsiConsole.Clear();
            var backups = GetBackupList();
            if (backups.Count == 0)
            {
                AnsiConsole.MarkupLine("[red]No backup jobs available.[/]");
                AnsiConsole.Markup("[bold yellow]Press Backspace to return to the main menu...[/]");
                while (Console.ReadKey(true).Key != ConsoleKey.Backspace) { }
                return null;
            }

            var selectedBackupName = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title(title)
                    .PageSize(10)
                    .AddChoices(backups.ConvertAll(b => b.Name)));

            var selectedBackup = backups.FirstOrDefault(b => b.Name == selectedBackupName);
            if (selectedBackup == null)
            {
                AnsiConsole.MarkupLine("[red]Selected backup not found.[/]");
                return null;
            }
            
            return selectedBackup;



        }
    }
}