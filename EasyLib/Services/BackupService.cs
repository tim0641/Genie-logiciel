using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Spectre.Console;
using EasyLib.Models;
using EasySaveLog.Models;
using EasySaveLog.Services;
using System.Reflection.Metadata.Ecma335;
using System.Drawing;
using System.Text;
using System.Windows;
using EasyLib;
using EasyLib.ViewModels;
using EasyLib.Services;
using System.Windows.Input;


namespace EasyLib.Services
{
    public class BackupService
    {
        private readonly string BackupFilePath = "backups.json"; // Path to the backup storage file
        private Dictionary<string, BackupModel> backups; // Stores backup instances
        private readonly DailyLogService _dailyLogService; // Service for logging backup events
        public string Status { get; private set; } // Stores the current backup status
        private static readonly object _lock = new object(); // Lock object for thread safety
        private readonly StateService _stateService; // Manages backup states
        private bool _isRunning; // Indicates if a backup process is running

        // Constructor initializes services and loads existing backups
        public BackupService(DailyLogService dailyLogService, StateService stateService)
        {
            _dailyLogService = dailyLogService;
            _stateService = stateService;
            LoadBackups();
        }

        // Creates a new backup entry
        public string CreateBackup(string name, string srcPath, string destPath, string type, bool isEncrypted = false, bool isDecrypted = false)
        {
            // Check if a backup with the same name already exists
            if (backups.ContainsKey(name))
                return Localization.Get("backup_exists");

            try
            {
                // Validate the source and destination paths
                if (File.Exists(srcPath))
                {
                    ValidatePath(srcPath, false); // Source is a file
                }
                else
                {
                    ValidatePath(srcPath, true); // Source is a directory
                }
                ValidatePath(destPath, true); // Ensure the destination is valid
            }
            catch (Exception)
            {
                // Log failure and stop any active timers
                _stateService.TakeAndUpdateStates(name, srcPath, destPath, Localization.Get("create failed"), type, 0, 0, 0, 0);
                _stateService.StopTimer();
                return Localization.Get("wrong_path");
            }

            // Create a new backup instance
            var backup = new BackupModel(name, srcPath, destPath, type, DateTime.Now, isEncrypted, isDecrypted);
            backups[name] = backup; // Store it in the dictionary
            SaveBackups(); // Persist changes to file

            // Calculate file size
            long fileSize = GetSize(srcPath, destPath, type);

            Console.WriteLine(name, srcPath, destPath, fileSize, "Create");

            // Log backup creation
            _dailyLogService.WriteLogEntry(new LogEntry
            {
                Timestamp = DateTime.Now,
                BackupName = name,
                SourcePath = srcPath,
                DestinationPath = destPath,
                FileSize = fileSize,
                Type = "Create"
            });

            _dailyLogService.FlushLogs(); // Flush log entries

            // Count total files in the backup
            long totalFiles = backup.IsDirectory ? CountFilesInDirectory(backup.SourcePath) : 1;

            // Update state and stop the timer
            _stateService.TakeAndUpdateStates(name, srcPath, destPath, Localization.Get("backup_success"), type, totalFiles, fileSize, 0, 100);
            _stateService.StopTimer();

            return Localization.Get("backup_success");
        }

        // Returns all saved backups
        public List<BackupModel> GetAllBackups()
        {
            return new List<BackupModel>(backups.Values);
        }

        // Runs a specified backup
        public string RunBackup(BackupModel backups, EncoursModel encours, PriorityModel priority, bool isEncrypted = false, bool isDecrypted = false)
        {
            List<string> statuses = new List<string>();

            // Check if the backup exists
            if (backups == null)
            {
                lock (statuses)
                {
                    statuses.Add($"{backups.Name} - {Localization.Get("no_backups")}");
                }
                return string.Join("\n", statuses);
            }

            long fileSize = GetSize(backups.SourcePath, backups.DestinationPath, backups.BackupType);
            long encryptionTimeMs = 0;

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            try
            {
                // Validate source path before processing
                if (File.Exists(backups.SourcePath))
                {
                    ValidatePath(backups.SourcePath, false);
                }
                else
                {
                    ValidatePath(backups.SourcePath, true);
                }

                if (backups.IsDirectory)
                {
                    long totalFiles = CountFilesInDirectory(backups.SourcePath);
                    CopyDirectory(encours, priority, backups.SourcePath, backups.DestinationPath, backups.BackupType, backups.Name, backups.BackupType, fileSize, isEncrypted, isDecrypted, ref encryptionTimeMs);
                    _stateService.StartTimer(backups.Name, backups.SourcePath, backups.DestinationPath, Localization.Get("backup_run_success"), backups.BackupType, totalFiles, fileSize, 0, 100);
                }
                else
                {
                    // Wait if the process is paused
                    while (!encours.EnCoursbool)
                    {
                        Thread.Sleep(500);
                    }
                    long totalFiles = 1;
                    Directory.CreateDirectory(Path.GetDirectoryName(backups.FullDestinationPath));
                    CopyFile(backups.SourcePath, backups.FullDestinationPath, backups.BackupType, backups.IsEncrypted, backups.IsDecrypted, ref encryptionTimeMs);
                    _stateService.StartTimer(backups.Name, backups.SourcePath, backups.DestinationPath, Localization.Get("backup_run_success"), backups.BackupType, totalFiles, fileSize, 0, 100);
                    encours.Progress = 100;
                }

                stopwatch.Stop();

                // Log backup execution
                _dailyLogService.WriteLogEntry(new LogEntry
                {
                    Timestamp = DateTime.Now,
                    BackupName = backups.Name,
                    SourcePath = backups.SourcePath,
                    DestinationPath = backups.DestinationPath,
                    FileSize = fileSize,
                    Time = stopwatch.ElapsedMilliseconds + "ms",
                    Type = "Run",
                    EncryptionTime = isEncrypted ? encryptionTimeMs + "ms" : null,
                    DecryptionTime = isDecrypted ? encryptionTimeMs + "ms" : null
                });
                _dailyLogService.FlushLogs();
                _stateService.StopTimer();

                lock (statuses)
                {
                    statuses.Add($"{backups.Name} ({backups.BackupType}) - {Localization.Get("backup_run_success")}");
                }
            }
            catch (Exception ex)
            {
                lock (statuses)
                {
                    statuses.Add($"{backups.Name} - [red]{ex.Message}[/]");
                }
            }

            Status = string.Join("\n", statuses);
            return Status;
        }

        // Deletes the specified backups
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
                                long totalFiles = CountFilesInDirectory(backup.SourcePath);
                                long filesLeftToDo = totalFiles;

                                foreach (var file in Directory.GetFiles(directoryToDelete, "*", SearchOption.AllDirectories))
                                {
                                    File.Delete(file);
                                    _stateService.StartTimer(name, backup.SourcePath, backup.DestinationPath, Localization.Get("delete_progress"), backup.BackupType, totalFiles, fileSize, --filesLeftToDo, (long)((double)(totalFiles - filesLeftToDo) / totalFiles * 100));
                                }

                                Directory.Delete(directoryToDelete, true);
                                _stateService.TakeAndUpdateStates(name, backup.SourcePath, backup.DestinationPath, Localization.Get("directory_successfully_deleted"), backup.BackupType, totalFiles, fileSize, filesLeftToDo, 100);
                                statuses.Add($"{name} - {Localization.Get("directory_successfully_deleted")}");
                            }
                            else
                            {
                                statuses.Add($"{name} - {Localization.Get("directory_delete_not_found")}");
                            }
                        }
                        else if (File.Exists(backup.FullDestinationPath))
                        {
                            File.Delete(backup.FullDestinationPath);
                            _stateService.StartTimer(name, backup.SourcePath, backup.DestinationPath, Localization.Get("file_successfully_deleted"), backup.BackupType, 1, fileSize, 0, 100);
                            statuses.Add($"{name} - {Localization.Get("file_successfully_deleted")}");
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
            return string.Join("\n", statuses);
        }
public event Action<long> ProgressUpdated;

// Method to invoke the ProgressUpdated event
protected virtual void OnProgressUpdated(long progress)
{
    ProgressUpdated?.Invoke(progress);
}

// Method to copy a directory with priority handling and encryption/decryption
private void CopyDirectory(EncoursModel encours, PriorityModel priority, string sourceDir, string destDir, string backupType, string name, string type, long filesize, bool isEncrypted, bool isDecrypted, ref long encryptionTimeMs)
{
    var destDirWithSource = Path.Combine(destDir, Path.GetFileName(sourceDir));
    Directory.CreateDirectory(destDirWithSource);

    long copiedFiles = 0;
    long totalFiles = CountFilesInDirectory(sourceDir);

    // Create directories in the destination path
    foreach (var dir in Directory.GetDirectories(sourceDir, "*", SearchOption.AllDirectories))
    {
        Directory.CreateDirectory(dir.Replace(sourceDir, destDirWithSource));
    }

    _stateService.StopTimer();

    // Retrieve all files from the source directory
    var allFiles = Directory.GetFiles(sourceDir, "*.*", SearchOption.AllDirectories);

    // Separate files into prioritized and non-prioritized lists based on the priority model
    var prioritizedFiles = new List<string>();
    var nonPrioritizedFiles = new List<string>();

    foreach (var file in allFiles)
    {
        string ext = Path.GetExtension(file).ToLower();
        bool isPriority = false;
        if (ext == ".docx" && priority.IsDocx)
            isPriority = true;
        else if (ext == ".pdf" && priority.IsPdf)
            isPriority = true;
        else if (ext == ".txt" && priority.IsTxt)
            isPriority = true;
        else if (ext == ".jpg" && priority.IsJpg)
            isPriority = true;
        // Add other conditions as needed

        if (isPriority)
            prioritizedFiles.Add(file);
        else
            nonPrioritizedFiles.Add(file);
    }

    // Concatenate the list: prioritized files first
    var filesToCopy = new List<string>();
    filesToCopy.AddRange(prioritizedFiles);
    filesToCopy.AddRange(nonPrioritizedFiles);

    long filesLeftToDo = totalFiles;

    foreach (var file in filesToCopy)
    {
        if (encours.Cancelled)
            break;

        while (!encours.EnCoursbool && !encours.Cancelled)
        {
            Thread.Sleep(500);
        }

        if (encours.Cancelled)
            break;

        string destinationFilePath = file.Replace(sourceDir, destDirWithSource);
        CopyFile(file, destinationFilePath, backupType, isEncrypted, isDecrypted, ref encryptionTimeMs);
        copiedFiles++;
        filesLeftToDo = totalFiles - copiedFiles;
        long progression = (long)((double)copiedFiles / totalFiles * 100);
        encours.Progress = progression;

        // Update the interface and backup state
        OnProgressUpdated(progression);
        _stateService.TakeAndUpdateStates(name, sourceDir, destDirWithSource, "Run in progress", type, totalFiles, filesize, filesLeftToDo, progression);
    }

    // If the backup was cancelled and the directory exists, delete it to avoid partially copied files
    if (encours.Cancelled && Directory.Exists(destDirWithSource))
    {
        Directory.Delete(destDirWithSource, true);
    }

    // Stop the state update timer
    _stateService.StopTimer();
}

// Method to copy a file with optional encryption/decryption

private void CopyFile(string sourceFile, string destFile, string backupType, bool isEncrypted, bool isDecrypted, ref long encryptionTimeMs)
{
    // Check if the backup type is "full" (in English or French) or if the destination file does not exist.
    // If true, copy the source file to the destination, overwriting if necessary.
    if (backupType.ToLower() == "full" || backupType.ToLower() == "complète" || !File.Exists(destFile))
    {
        File.Copy(sourceFile, destFile, true);
    }
    else
    {
        var sourceInfo = new FileInfo(sourceFile);
        var destInfo = new FileInfo(destFile);

        // Copy the file only if it has been modified (last write time is newer) or its size has changed.
        if (sourceInfo.LastWriteTime > destInfo.LastWriteTime || sourceInfo.Length != destInfo.Length)
        {
            File.Copy(sourceFile, destFile, true);
        }
    }

    // Handle encryption or decryption if needed.
    if (isEncrypted || isDecrypted)
    {
        string encryptionKey = "MaCleSecrete64Bits"; // Secret encryption key
        string cryptoSoftPath = @"C:\Genie-logiciel\CryptoSoft\CryptoSoft.csproj"; // Path to encryption software
        string mode = isEncrypted ? "--encrypt" : isDecrypted ? "--decrypt" : ""; // Determine encryption or decryption mode
        
        // Measure encryption or decryption time and add it to the total encryption time.
        long cryptoTime = EncryptOrDecryptFile(destFile, encryptionKey, cryptoSoftPath, mode);
        encryptionTimeMs += cryptoTime;
    }
}

public void CancelBackup(BackupModel backup)
{
    // If the backup object is null, return immediately.
    if (backup == null)
        return;

    // Check if the backup is a directory or a file.
    if (backup.IsDirectory)
    {
        // Construct the destination directory path based on the source directory name.
        string destDirWithSource = Path.Combine(backup.DestinationPath, Path.GetFileName(backup.SourcePath));

        // If the directory exists, attempt to delete it.
        if (Directory.Exists(destDirWithSource))
        {
            try { Directory.Delete(destDirWithSource, true); }
            catch (Exception ex) { /* Handle the error if necessary */ }
        }
    }
    else
    {
        // If the backup is a file, check if it exists before attempting to delete it.
        if (File.Exists(backup.FullDestinationPath))
        {
            try { File.Delete(backup.FullDestinationPath); }
            catch (Exception ex) { /* Handle the error if necessary */ }
        }
    }
}

private void SaveBackups()
{
    lock (_lock) // Lock to ensure thread safety when writing to the backup file.
    {
        // Serialize backup data to JSON format.
        var json = JsonSerializer.Serialize(backups, new JsonSerializerOptions { WriteIndented = true });
        
        // Save the serialized JSON string to the backup file.
        File.WriteAllText(BackupFilePath, json);
    }
}

private void LoadBackups()
{
    // Check if the backup file exists before attempting to read it.
    if (File.Exists(BackupFilePath))
    {
        // Read and deserialize the backup data from the JSON file.
        var json = File.ReadAllText(BackupFilePath);
        backups = JsonSerializer.Deserialize<Dictionary<string, BackupModel>>(json) ?? new Dictionary<string, BackupModel>();
    }
    else
    {
        // If no backup file exists, initialize an empty dictionary.
        backups = new Dictionary<string, BackupModel>();
    }
}

public long GetSize(string sourcePath, string destPath, string backupType)
{
    // Determine whether the path is a file or a directory and calculate its size accordingly.
    if (File.Exists(sourcePath))
    {
        return GetFileSize(sourcePath, destPath, backupType);
    }
    else if (Directory.Exists(sourcePath))
    {
        var result = GetDirectorySize(new DirectoryInfo(sourcePath), destPath, backupType);
        return result[0]; // Return total size of directory.
    }
    return 0; // Return 0 if the path does not exist.
}

private long GetFileSize(string sourcePath, string destPath, string backupType)
{
    string destinationFile = Path.Combine(destPath, Path.GetFileName(sourcePath));

    // If it's a full backup or the file does not exist, return its size.
    if (backupType.ToLower() == "full" || backupType.ToLower() == "complète" || !File.Exists(destinationFile))
    {
        return new FileInfo(sourcePath).Length;
    }
    else
    {
        return IsFileModified(sourcePath, destinationFile) ? new FileInfo(sourcePath).Length : 0;
    }
}

private List<long> GetDirectorySize(DirectoryInfo sourceDir, string destPath, string backupType)
{
    List<long> result = new List<long> { 0, 0 };

    Stack<DirectoryInfo> stack = new Stack<DirectoryInfo>();
    stack.Push(sourceDir);

    while (stack.Count > 0)
    {
        DirectoryInfo currentDir = stack.Pop();
        string relativePath = Path.GetRelativePath(sourceDir.FullName, currentDir.FullName);
        string destDirPath = Path.Combine(destPath, Path.GetFileName(sourceDir.FullName), relativePath);

        foreach (var subDir in currentDir.GetDirectories())
        {
            stack.Push(subDir);
        }

        // Process all files in the directory and update the result.
        result = ProcessFilesInDirectory(currentDir, destDirPath, sourceDir.FullName, destPath, backupType);
    }

    return result;
}

private bool IsFileModified(string sourceFile, string destFile)
{
    if (!File.Exists(destFile))
        return true; // The file is considered modified if it doesn't exist.

    var sourceInfo = new FileInfo(sourceFile);
    var destInfo = new FileInfo(destFile);

    // Compare file sizes and last modification dates.
    return sourceInfo.Length != destInfo.Length || sourceInfo.LastWriteTime > destInfo.LastWriteTime;
}

private void ValidatePath(string path, bool isDirectory = false)
{
    // Validate the given path to ensure it is not empty or invalid.
    if (string.IsNullOrWhiteSpace(path))
    {
        Console.WriteLine("Path is empty");
        throw new ArgumentException(Localization.Get("error_path_empty"));
    }

    // Check if the directory exists when expected.
    if (isDirectory && !Directory.Exists(path))
    {
        Console.WriteLine("Directory does not exist");
        throw new DirectoryNotFoundException($"{Localization.Get("error_directory_not_found")}: {path}");
    }

    // Check if the file exists when expected.
    if (!isDirectory && !File.Exists(path))
    {
        Console.WriteLine("File does not exist");
        throw new FileNotFoundException($"{Localization.Get("error_file_not_found")}: {path}");
    }
}

public int CountFilesInDirectory(string directoryPath)
{
    // Ensure the directory exists before attempting to count files.
    if (!Directory.Exists(directoryPath))
    {
        throw new DirectoryNotFoundException($"The directory {directoryPath} does not exist.");
    }
    
    // Count all files within the directory and its subdirectories.
    return Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories).Length;
}
private List<long> ProcessFilesInDirectory(DirectoryInfo currentDir, string destDirPath, string sourcePath, string destPath, string backupType)
{
    List<long> result = new List<long> { 0, 0 };
    long fileCount = 0;

    foreach (var file in currentDir.GetFiles())
    {
        string relativeFilePath = Path.GetRelativePath(sourcePath, file.FullName);
        string correspondingDestFile = Path.Combine(destPath, Path.GetFileName(sourcePath), relativeFilePath);
        
        // Calculate total size based on backup type and modifications
        if (backupType.ToLower() == "full" || backupType.ToLower() == "complète" || !File.Exists(correspondingDestFile))
        {
            result[0] += file.Length;
        }
        else
        {
            if (IsFileModified(file.FullName, correspondingDestFile))
            {
                result[0] += file.Length;
            }
        }
        fileCount++;
    }

    return result;
}
private long EncryptOrDecryptFile(string filePath, string encryptionKey, string cryptoSoftPath, string mode)
{
    Stopwatch stopwatch = new Stopwatch();
    stopwatch.Start(); // Start timing the encryption/decryption process.

    ProcessStartInfo psi = new ProcessStartInfo
    {
        FileName = "dotnet",
        Arguments = $"run --project \"{cryptoSoftPath}\" \"{filePath}\" \"{encryptionKey}\" {mode}",
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false,
        CreateNoWindow = true
    };

    using (Process process = new Process { StartInfo = psi })
    {
        process.Start();
        string output = process.StandardOutput.ReadToEnd();
        string error = process.StandardError.ReadToEnd();
        process.WaitForExit();
    }

    stopwatch.Stop();
    return stopwatch.ElapsedMilliseconds; // Return the total time taken for encryption or decryption.
}}}