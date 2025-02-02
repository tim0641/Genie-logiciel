using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using EasyLib.Models;
using EasySaveLog.Models;
using EasySaveLog.Services;

namespace EasyLib.Services
{
    public class BackupService
    {
        private readonly string BackupFilePath = "backups.json";
        private Dictionary<string, BackupModel> backups;
        private readonly DailyLogService _dailyLogService;
        public string Status { get; private set; }
        private static readonly object _lock = new object();

        public BackupService()
        {
            _dailyLogService = new DailyLogService(@"C:\Logs\Daily");
            LoadBackups();
        }

        public string CreateBackup(string name, string srcPath, string destPath, string type)
        {
            if (backups.ContainsKey(name))
                return Localization.Get("backup_exists");

            try
            {
                if(File.Exists(srcPath))
                {
                    ValidatePath(srcPath, false);
                }
                else
                {
                    ValidatePath(srcPath, true);;
                }
                
                ValidatePath(destPath, true);
            }
            catch (Exception)
            {
                return Localization.Get("wrong_path");
            }

            var backup = new BackupModel(name, srcPath, destPath, type, DateTime.Now);
            backups[name] = backup;
            SaveBackups();

            long fileSize = GetSize(srcPath, destPath, type);
            _dailyLogService.WriteLogEntry(new LogEntry
            {
                Timestamp = DateTime.Now,
                BackupName = name,
                SourcePath = srcPath,
                DestinationPath = destPath,
                FileSize = fileSize,
                Type = "Create"
            });
            _dailyLogService.FlushLogs();

            return Localization.Get("backup_success");
        }

        public List<BackupModel> GetAllBackups()
        {
            return new List<BackupModel>(backups.Values);
        }

        public string RunBackup(List<string> backupNames)
        {
            List<string> statuses = new List<string>();

            Parallel.ForEach(backupNames, name =>
            {
                if (!backups.ContainsKey(name))
                {
                    lock (statuses)
                    {
                        statuses.Add($"{name} - {Localization.Get("no_backups")}");
                    }
                    return;
                }

                var backup = backups[name];
                long fileSize = GetSize(backup.SourcePath, backup.DestinationPath, backup.BackupType);
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                try
                {
                if(File.Exists(backup.SourcePath))
                {
                    ValidatePath(backup.SourcePath, false);
                }
                else
                {
                    ValidatePath(backup.SourcePath, true);
                }

                    if (backup.IsDirectory)
                    {
                        CopyDirectory(backup.SourcePath, backup.DestinationPath, backup.BackupType);
                    }
                    else
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(backup.FullDestinationPath));
                        CopyFile(backup.SourcePath, backup.FullDestinationPath, backup.BackupType);
                    }

                    stopwatch.Stop();

                    _dailyLogService.WriteLogEntry(new LogEntry
                    {
                        Timestamp = DateTime.Now,
                        BackupName = backup.Name,
                        SourcePath = backup.SourcePath,
                        DestinationPath = backup.DestinationPath,
                        FileSize = fileSize,
                        Time = stopwatch.ElapsedMilliseconds + "ms",
                        Type = "Run"
                    });
                    _dailyLogService.FlushLogs();

                    lock (statuses)
                    {
                        statuses.Add($"{backup.Name} ({backup.BackupType}) - {Localization.Get("backup_run_success")}");
                    }
                }
                catch (Exception ex)
                {
                    lock (statuses)
                    {
                        statuses.Add($"{backup.Name} - [red]{ex.Message}[/]");
                    }
                }
            });

            Status = string.Join("\n", statuses);
            return Status;
        }

        public string DeleteBackup(List<string> backupNames)
        {
            List<string> statuses = new List<string>();

            foreach (var name in backupNames)
            {
                lock (_lock)
                {
                    if (!backups.ContainsKey(name))
                    {
                        statuses.Add($"{name} - {Localization.Get("backup_not_found")}");
                        continue;
                    }

                    var backup = backups[name];
                    long fileSize = GetSize(backup.SourcePath, backup.DestinationPath, backup.BackupType);

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
                                statuses.Add($"{name} - {Localization.Get("directory_sucessfully_deleted")}");
                            }
                            else
                            {
                                statuses.Add($"{name} - {Localization.Get("directory_delete_not_found")}");
                            }
                        }
                        else if (File.Exists(backup.FullDestinationPath))
                        {
                            File.Delete(backup.FullDestinationPath);
                            statuses.Add($"{name} - {Localization.Get("file_sucessfully_deleted")}");
                        }
                        else
                        {
                            statuses.Add($"{name} - {Localization.Get("file_delete_not_found")}");
                        }

                        backups.Remove(name);
                        SaveBackups();

                        _dailyLogService.WriteLogEntry(new LogEntry
                        {
                            Timestamp = DateTime.Now,
                            BackupName = backup.Name,
                            SourcePath = backup.SourcePath,
                            DestinationPath = backup.DestinationPath,
                            FileSize = fileSize,
                            Type = "Delete"
                        });
                        _dailyLogService.FlushLogs();
                    }
                    catch (Exception ex)
                    {
                        statuses.Add($"{name} - {Localization.Get("error_delete")}: {ex.Message}");
                    }
                }
            }
            Status = string.Join("\n", statuses);
            return Status;
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
            if (backupType.ToLower() == "full"|| backupType.ToLower() == "complète" || !File.Exists(destFile))
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

        private void SaveBackups()
        {
            lock (_lock)
            {
                var json = JsonSerializer.Serialize(backups, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(BackupFilePath, json);
            }
        }

        private void LoadBackups()
        {
            if (File.Exists(BackupFilePath))
            {
                var json = File.ReadAllText(BackupFilePath);
                backups = JsonSerializer.Deserialize<Dictionary<string, BackupModel>>(json) ?? new Dictionary<string, BackupModel>();
            }
            else
            {
                backups = new Dictionary<string, BackupModel>();
            }
        }

        public long GetSize(string sourcePath, string destPath, string backupType)
        {
            if (File.Exists(sourcePath))
            {
                return GetFileSize(sourcePath, destPath, backupType);
            }
            else if (Directory.Exists(sourcePath))
            {
                return GetDirectorySize(new DirectoryInfo(sourcePath), destPath, backupType);
            }
            return 0;
        }

        private long GetFileSize(string sourcePath, string destPath, string backupType)
        {
            string destinationFile = Path.Combine(destPath, Path.GetFileName(sourcePath));

            if (backupType.ToLower() == "full" || backupType.ToLower() == "complète"|| !File.Exists(destinationFile))
            {
                return new FileInfo(sourcePath).Length;
            }
            else
            {
                return IsFileModified(sourcePath, destinationFile) ? new FileInfo(sourcePath).Length : 0;
            }
        }

        private long GetDirectorySize(DirectoryInfo sourceDir, string destPath, string backupType)
        {
            long totalSize = 0;
            Stack<DirectoryInfo> stack = new Stack<DirectoryInfo>();
            stack.Push(sourceDir);

            while (stack.Count > 0)
            {
                DirectoryInfo currentDir = stack.Pop();
                string relativePath = Path.GetRelativePath(sourceDir.FullName, currentDir.FullName);
                string destDirPath = Path.Combine(destPath, Path.GetFileName(sourceDir.FullName), relativePath);

                totalSize += ProcessFilesInDirectory(currentDir, destDirPath, sourceDir.FullName, destPath, backupType);

                foreach (var subDir in currentDir.GetDirectories())
                {
                    stack.Push(subDir);
                }
            }

            return totalSize;
        }

        private long ProcessFilesInDirectory(DirectoryInfo currentDir, string destDirPath, string sourcePath, string destPath, string backupType)
        {
            long size = 0;
            foreach (var file in currentDir.GetFiles())
            {
                string relativeFilePath = Path.GetRelativePath(sourcePath, file.FullName);
                string correspondingDestFile = Path.Combine(destPath, Path.GetFileName(sourcePath), relativeFilePath);
                if (backupType.ToLower() == "full"|| backupType.ToLower() == "complète" || !File.Exists(correspondingDestFile))
                {
                    size += file.Length;
                }
                else
                {
                    if (IsFileModified(file.FullName, correspondingDestFile))
                    {
                        size += file.Length;
                    }
                }
            }
            return size;
        }

        private bool IsFileModified(string sourceFile, string destFile)
        {
            if (!File.Exists(destFile))
                return true;

            var sourceInfo = new FileInfo(sourceFile);
            var destInfo = new FileInfo(destFile);

            return sourceInfo.Length != destInfo.Length || sourceInfo.LastWriteTime > destInfo.LastWriteTime;
        }

        private void ValidatePath(string path, bool isDirectory = false)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                Console.WriteLine("Path is empty");
                throw new ArgumentException(Localization.Get("error_path_empty"));
            }

            if (isDirectory && !Directory.Exists(path))
            {
                Console.WriteLine("Directory does not exist");
                throw new DirectoryNotFoundException($"{Localization.Get("error_directory_not_found")}: {path}");
            }

            if (!isDirectory && !File.Exists(path))
            {
                Console.WriteLine("File does not exist");
                throw new FileNotFoundException($"{Localization.Get("error_file_not_found")}: {path}");
            }
        }
    }
}