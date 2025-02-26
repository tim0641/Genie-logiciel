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
        private readonly string BackupFilePath = "backups.json";
        private Dictionary<string, BackupModel> backups;
        private readonly DailyLogService _dailyLogService;
        public string Status { get; private set; }
        private static readonly object _lock = new object();
        private readonly StateService _stateService;

        private bool _isRunning;

        public BackupService(DailyLogService dailyLogService, StateService stateService)
        {
            _dailyLogService = dailyLogService;
            _stateService = stateService;
            LoadBackups();
        }

        public string CreateBackup(string name, string srcPath, string destPath, string type, bool isEncrypted = false, bool isDecrypted=false)
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
                 _stateService.TakeAndUpdateStates(name, srcPath, destPath, Localization.Get("create failed"), type, 0 , 0, 0, 0);
                 _stateService.StopTimer();
                return Localization.Get("wrong_path");

            }

            var backup = new BackupModel(name, srcPath, destPath, type, DateTime.Now, isEncrypted, isDecrypted);

            backups[name] = backup;
            SaveBackups();

            long fileSize = GetSize(srcPath, destPath, type);

            Console.WriteLine( name, srcPath, destPath, fileSize, "Create");

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
            long totalfiles = 0;
            if (backup.IsDirectory)
            {
                  totalfiles = CountFilesInDirectory(backup.SourcePath);
            }
            else
            {

                  totalfiles =1;
            }
_stateService.TakeAndUpdateStates(name, srcPath, destPath, Localization.Get("backup_success"), type, totalfiles, fileSize, 0, 100);
_stateService.StopTimer();


            return Localization.Get("backup_success");
        }

        public List<BackupModel> GetAllBackups()
        {
            return new List<BackupModel>(backups.Values);
        }

        public string RunBackup(BackupModel backups, EncoursModel encours, PriorityModel priority ,bool isEncrypted=false, bool isDecrypted=false)
        {
            List<string> statuses = new List<string>();

  
            if (backups == null)
            {
                lock (statuses)
                {
                    statuses.Add($"{backups.Name} - {Localization.Get("no_backups")}");
                }
                return string.Join("\n", statuses); // Retourne le statut si le backup n'existe pas
            }

                
                long fileSize = GetSize(backups.SourcePath, backups.DestinationPath, backups.BackupType);
                long encryptionTimeMs = 0; 

                var stopwatch = new Stopwatch();
                stopwatch.Start();
                try
                {
                if(File.Exists(backups.SourcePath))
                {
                    ValidatePath(backups.SourcePath, false);
                }
                else
                {
                    ValidatePath(backups.SourcePath, true);
                }
                    if (backups.IsDirectory)
                    {


                        long totalfiles = CountFilesInDirectory(backups.SourcePath);
                        CopyDirectory(encours, priority, backups.SourcePath, backups.DestinationPath, backups.BackupType, backups.Name, backups.BackupType, fileSize, isEncrypted, isDecrypted, ref encryptionTimeMs);
                        _stateService.StartTimer(backups.Name, backups.SourcePath, backups.DestinationPath, Localization.Get("backup_run_success"), backups.BackupType, totalfiles, fileSize, 0, 100);
                                           


                    }
                    else
                    {      
                        while (!encours.EnCoursbool) {
                            Thread.Sleep(500);
                        }          
                        long totalfiles = 1;    
                        Directory.CreateDirectory(Path.GetDirectoryName(backups.FullDestinationPath));
                        CopyFile(backups.SourcePath, backups.FullDestinationPath, backups.BackupType,backups.IsEncrypted, backups.IsDecrypted, ref encryptionTimeMs);
                        _stateService.StartTimer(backups.Name, backups.SourcePath, backups.DestinationPath, Localization.Get("backup_run_success"), backups.BackupType, totalfiles, fileSize, 0, 100);
                        encours.Progress = 100;

                    }
                    stopwatch.Stop();
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
                                long totalfiles = CountFilesInDirectory(backup.SourcePath);

                                long deleteFiles = 0;
                                long? filesLeftToDo = totalfiles; 
                                deleteFiles++;
                                filesLeftToDo = totalfiles-deleteFiles;

                                long progression = (long)((double)deleteFiles / totalfiles * 100);

                                foreach (var file in Directory.GetFiles(directoryToDelete, "*", SearchOption.AllDirectories))
                                {
                                    File.Delete(file);
                                     _stateService.StartTimer(name, backup.SourcePath, backup.DestinationPath, Localization.Get("delete_progress"), backup.BackupType, totalfiles, fileSize, filesLeftToDo, progression);

                                // Thread.Sleep(100);
                                }

                                foreach (var dir in Directory.GetDirectories(directoryToDelete, "*", SearchOption.AllDirectories))
                                {
                                    Directory.Delete(dir, true);

                                }
                                Directory.Delete(directoryToDelete, true);
                                 _stateService.TakeAndUpdateStates(name, backup.SourcePath, backup.DestinationPath, Localization.Get("directory_sucessfully_deleted"), backup.BackupType, totalfiles, fileSize, filesLeftToDo, 100);
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
                             _stateService.StartTimer(name, backup.SourcePath, backup.DestinationPath, Localization.Get("file_sucessfully_deleted"), backup.BackupType, 1, fileSize, 0, 100);


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

                        //  _stateService.StopTimer();

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


        public event Action<long> ProgressUpdated;
        protected virtual void OnProgressUpdated(long progress)
        {
            ProgressUpdated?.Invoke(progress);
        }


        private void CopyDirectory(EncoursModel encours, PriorityModel priority, string sourceDir, string destDir, string backupType, string name, string type, long filesize, bool isEncrypted , bool isDecrypted, ref long encryptionTimeMs )
        {
            var destDirWithSource = Path.Combine(destDir, Path.GetFileName(sourceDir));
            Directory.CreateDirectory(destDirWithSource);
                                          


            long copiedFiles = 0;
            long totalFiles = CountFilesInDirectory(sourceDir);


            foreach (var dir in Directory.GetDirectories(sourceDir, "*", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(dir.Replace(sourceDir, destDirWithSource));

            }

            _stateService.StopTimer();


    // Récupérer tous les fichiers du répertoire source
    var allFiles = Directory.GetFiles(sourceDir, "*.*", SearchOption.AllDirectories);
    
    // Séparer les fichiers prioritaires et non prioritaires selon le modèle
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
        // Vous pouvez ajouter d'autres conditions selon vos besoins

        if (isPriority)
            prioritizedFiles.Add(file);
        else
            nonPrioritizedFiles.Add(file);
    }

    // Concaténer la liste : les fichiers prioritaires en premier
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

    // Mise à jour de l'interface et de l'état de la sauvegarde
    OnProgressUpdated(progression);
    _stateService.TakeAndUpdateStates(name, sourceDir, destDirWithSource, "Run en cours", type, totalFiles, filesize, filesLeftToDo, progression);


}

// Si la sauvegarde a été annulée et que le dossier existe, le supprimer pour éviter les fichiers partiellement copiés
if (encours.Cancelled && Directory.Exists(destDirWithSource))
{
    Directory.Delete(destDirWithSource, true);
}

// Arrêter le timer de mise à jour des états
_stateService.StopTimer();

        }


        private void CopyFile(string sourceFile, string destFile, string backupType, bool isEncrypted , bool isDecrypted, ref long encryptionTimeMs)
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

                if (isEncrypted || isDecrypted) 
                {
                string encryptionKey = "MaCleSecrete64Bits";
                string cryptoSoftPath= PathHelper.GetCryptoSoftProjectPath();
                string mode = isEncrypted ? "--encrypt" : isDecrypted ? "--decrypt" : "";
        long cryptoTime = EncryptOrDecryptFile(destFile, encryptionKey, cryptoSoftPath, mode);
        encryptionTimeMs += cryptoTime;
    }
        }
public void CancelBackup(BackupModel backup)
{
    if (backup == null)
        return;
    
    if (backup.IsDirectory)
    {
        string destDirWithSource = Path.Combine(backup.DestinationPath, Path.GetFileName(backup.SourcePath));
        if (Directory.Exists(destDirWithSource))
        {
            try { Directory.Delete(destDirWithSource, true); }
            catch (Exception ex) { /* Gérer l'erreur si nécessaire */ }
        }
    }
    else
    {
        if (File.Exists(backup.FullDestinationPath))
        {
            try { File.Delete(backup.FullDestinationPath); }
            catch (Exception ex) { /* Gérer l'erreur si nécessaire */ }
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
                var result = GetDirectorySize(new DirectoryInfo(sourcePath), destPath, backupType);
                return result[0];
            }
            return 0;
        }

        private long GetFileSize(string sourcePath, string destPath, string backupType)
        {
            string destinationFile = Path.Combine(destPath, Path.GetFileName(sourcePath));

            if (backupType.ToLower() == "full" || backupType.ToLower() == "complÃ¨te"|| !File.Exists(destinationFile))
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
             result = ProcessFilesInDirectory(currentDir, destDirPath, sourceDir.FullName, destPath, backupType);
            }

            return result;
        }

        private List<long> ProcessFilesInDirectory(DirectoryInfo currentDir, string destDirPath, string sourcePath, string destPath, string backupType)
        {
            
            List<long> result = new List<long> { 0, 0 };  
            long fileCount = 0; 

            foreach (var file in currentDir.GetFiles())
            {
                string relativeFilePath = Path.GetRelativePath(sourcePath, file.FullName);
                string correspondingDestFile = Path.Combine(destPath, Path.GetFileName(sourcePath), relativeFilePath);
                if (backupType.ToLower() == "full"|| backupType.ToLower() == "complÃ¨te" || !File.Exists(correspondingDestFile))
                {
                    result[0] += file.Length;
                }
                else
                {
                    if (IsFileModified(file.FullName, correspondingDestFile))
                    {
                        result[0]+= file.Length;
                    }
                }
                fileCount++;
            }

            return result;
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
        public int CountFilesInDirectory(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                throw new DirectoryNotFoundException($"Le répertoire {directoryPath} n'existe pas.");
            }
            return Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories).Length;
        }


private long EncryptOrDecryptFile(string filePath, string encryptionKey, string cryptoSoftPath, string mode)
{
    Stopwatch stopwatch = new Stopwatch();
    stopwatch.Start(); 
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
    long encryptionTime = stopwatch.ElapsedMilliseconds;
    return encryptionTime;
}

}}