using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Spectre.Console;
using EasyLib.Models;

namespace EasyLib.ViewModels
{
    public class BackupViewModel
    {
        private Dictionary<string, BackupModel> backups = new Dictionary<string, BackupModel>();
        private const string BackupFilePath = "backups.json";
        private static readonly object _lock = new object();
        public string Status { get; private set; }

        public BackupViewModel()
        {
            LoadBackups();
        }

        public BackupModel GetBackupByName(string name)
        {
            return backups.ContainsKey(name) ? backups[name] : null;
        }

        public void CreateBackup(string name, string srcPath, string destPath, string type)
        {
            if (backups.ContainsKey(name))
            {
                Status = Localization.Get("backup_exists");
                return;
            }

            try
            {
                ValidatePath(srcPath, true);  
                ValidatePath(destPath, true); 
            }
            catch (Exception ex)
            {
                Status = $"[red]{ex.Message}[/]";
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

            Status = $"{Localization.Get("list_of_backup_jobs")}\n";
            foreach (var backup in backups)
            {
                Status += $"{backup.Key} - {backup.Value.SourcePath} -> {backup.Value.FullDestinationPath} ({backup.Value.BackupType})\n";
            }
        }

        public void RunBackup(List<string> backupNames)
        {
            if (backupNames.Count == 0)
            {
                Status = Localization.Get("no_backups_selected");
                return;
            }

            List<string> allStatuses = new List<string>();

            Parallel.ForEach(backupNames, (name) =>
            {
                if (!backups.ContainsKey(name))
                {
                    lock (allStatuses)
                    {
                        allStatuses.Add($"{name} - {Localization.Get("backup_not_found")}");
                    }
                    return;
                }

                var backup = backups[name];

                try
                {
                    ValidatePath(backup.SourcePath, true);

                    if (backup.IsDirectory)
                    {
                        CopyDirectory(backup.SourcePath, backup.DestinationPath, backup.BackupType);
                    }
                    else
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(backup.FullDestinationPath));
                        CopyFile(backup.SourcePath, backup.FullDestinationPath, backup.BackupType);
                    }

                    lock (allStatuses)
                    {
                        allStatuses.Add($"{name} ({backup.BackupType}) - {Localization.Get("backup_run_success")}");
                    }
                }
                catch (Exception ex)
                {
                    lock (allStatuses)
                    {
                        allStatuses.Add($"{name} - [red]{ex.Message}[/]");
                    }
                }
            });

            Status = string.Join("\n", allStatuses);
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

                public void DeleteBackup(List<string> backupNames)
        {
            if (backupNames.Count == 0)
            {
                Status = Localization.Get("no_backups_selected_for_deletion");
                return;
            }

            List<string> allStatuses = new List<string>();

            Parallel.ForEach(backupNames, (name) =>
            {
                lock (_lock)
                {
                    if (!backups.ContainsKey(name))
                    {
                        allStatuses.Add($"{name} - {Localization.Get("backup_not_found")}");
                        return;
                    }

                    var backup = backups[name];

                    try
                    {
                        if (backup.IsDirectory)
                        {
                            string directoryToDelete = Path.Combine(backup.DestinationPath, Path.GetFileName(backup.SourcePath));

                            if (Directory.Exists(directoryToDelete))
                            {
                                foreach (var file in Directory.GetFiles(directoryToDelete, "*", SearchOption.AllDirectories))
                                {
                                    File.Delete(file);
                                }

                                foreach (var dir in Directory.GetDirectories(directoryToDelete, "*", SearchOption.AllDirectories))
                                {
                                    Directory.Delete(dir, true);
                                }

                                Directory.Delete(directoryToDelete, true);
                                allStatuses.Add($"{name} - {Localization.Get("directory_sucessfully_deleted")}");
                            }
                            else
                            {
                                allStatuses.Add($"{name} - {Localization.Get("directory_delete_not_found")}");
                            }
                        }
                        else if (File.Exists(backup.FullDestinationPath))
                        {
                            File.Delete(backup.FullDestinationPath);
                            allStatuses.Add($"{name} - {Localization.Get("file_sucessfully_deleted")}");
                        }
                        else
                        {
                            allStatuses.Add($"{name} - {Localization.Get("file_delete_not_found")}");
                        }

                        backups.Remove(name);
                        SaveBackups();
                    }
                    catch (Exception ex)
                    {
                        allStatuses.Add($"{name} - {Localization.Get("error_delete")}: {ex.Message}");
                    }
                }
            });

            Status = string.Join("\n", allStatuses);
        }

        private void SaveBackups()
        {
            lock (_lock)
            {
                var json = JsonSerializer.Serialize(backups, new JsonSerializerOptions { WriteIndented = true });

                try
                {
                    File.WriteAllText(BackupFilePath, json);
                }
                catch (IOException ex)
                {
                    Status = $"{Localization.Get("error_saving_backups")}: {ex.Message}";
                }
            }
        }

        private void LoadBackups()
        {
            if (File.Exists(BackupFilePath))
            {
                try
                {
                    using (FileStream fs = new FileStream(BackupFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    using (StreamReader reader = new StreamReader(fs))
                    {
                        string json = reader.ReadToEnd();
                        backups = JsonSerializer.Deserialize<Dictionary<string, BackupModel>>(json) ?? new Dictionary<string, BackupModel>();
                    }
                }
                catch (IOException ex)
                {
                    Status = $"{Localization.Get("error_loading_backups")}: {ex.Message}";
                }
            }
        }

        public List<BackupModel> GetBackupList()
        {
            return new List<BackupModel>(backups.Values);
        }

        private void ValidatePath(string path, bool isDirectory = false)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException(Localization.Get("error_path_empty"));
            }

            if (isDirectory && !Directory.Exists(path))
            {
                throw new DirectoryNotFoundException($"{Localization.Get("error_directory_not_found")}: {path}");
            }

            if (!isDirectory && !File.Exists(path))
            {
                throw new FileNotFoundException($"{Localization.Get("error_file_not_found")}: {path}");
            }
        }
    }
}
