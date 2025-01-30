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
                Status = Localization.Get("backup_exits");
                return;
            }

            var backup = new BackupModel(name, srcPath, destPath, type, DateTime.Now);
            backups[name] = backup;
            SaveBackups();
            Status = Localization.Get("backup_success");
        }

        public void ListBackups()
        {
            if (backups.Count == 0)
            {
                Status = Localization.Get("no_backupsjob");
                return;
            }

            Status =$"{Localization.Get("List_of_backup_job")}\n";
            foreach (var backup in backups)
            {
                Status += $"{backup.Key} - {backup.Value.SourcePath} -> {backup.Value.FullDestinationPath} ({backup.Value.BackupType})\n";
            }
        }

        public void RunBackup(string name)
        {
            if (!backups.ContainsKey(name))
            {
                Status = Localization.Get("backup_not_found");
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
                Status = $" {name} ({backup.BackupType}) {Localization.Get("backup_run_succes")}";
            }
            catch (Exception ex)
            {
                Status = $"{Localization.Get("backup_error")} {ex.Message}";
            }
        }

        public void RunAllBackups()
        {
            if (backups.Count == 0)
            {
                Status = Localization.Get("no_backups");
                return;
            }
            List<String> allStatus = new List<String>();

            Status = $"{Localization.Get("running_all_backups")}\n";
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
                Status = Localization.Get("backup_not_found");
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
                        Status = Localization.Get("Directory_delete");
                    }
                    else
                    {
                        Status = Localization.Get("Directory_delete_not_found");
                    }
                }
                else if (File.Exists(backup.FullDestinationPath))
                {
                    File.Delete(backup.FullDestinationPath);
                    Status = Localization.Get("file_delete");
                }
                else
                {
                    Status = Localization.Get("file_delete_not_found");
                }
            }
            catch (Exception ex)
            {
                Status = $" {Localization.Get("error_delete")} {ex.Message}";
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
                AnsiConsole.MarkupLine(Localization.Get("no_backups")); 
                AnsiConsole.Markup($"[bold]{ Localization.Get("press_backspace")}[/]");
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
                AnsiConsole.MarkupLine($"[red]{Localization.Get("press_backspace")}[/]");
                return null;
            }
            
            return selectedBackup;



        }
    }
}