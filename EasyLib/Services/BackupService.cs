using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Spectre.Console;
using EasyLib.Models;
using EasySaveLog.Models;
using EasySaveLog.Services;
using System.Reflection.Metadata.Ecma335;
 
namespace EasyLib.Services
{
    public class BackupService
    {
        private readonly string BackupFilePath = "backups.json";// Chemin du fichier où sont sauvegardées les sauvegardes
        private Dictionary<string, BackupModel> backups; // Dictionnaire stockant les sauvegardes
        private readonly DailyLogService _dailyLogService; // Service de journalisation
        public string Status { get; private set; } // État des opérations en cours
        private static readonly object _lock = new object(); // Objet de verrouillage pour les accès concurrents
        private readonly StateService _stateService; // Service de gestion des états


        // Constructeur initialisant les services de log et d'état et chargeant les sauvegardes
        public BackupService()
        {
            _dailyLogService = new DailyLogService(@"C:\Logs\Daily");
            _stateService = new StateService(@"C:\Logs\States\Daily");
            LoadBackups();
        }

            // Crée une sauvegarde en validant les chemins et en l'ajoutant à la liste des sauvegardes
        public string CreateBackup(string name, string srcPath, string destPath, string type)
        {

            if (backups.ContainsKey(name))
        
                return Localization.Get("backup_exists");

            try
            // Vérification de la validité des chemins source et destination
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
            // Mise à jour de l'état en cas d'échec de validation des chemins
            {
                _stateService.TakeAndUpdateStates(name, srcPath, destPath, Localization.Get("create failed"), type, 0 , 0, 0, 0);
                _stateService.StopTimer();
                return Localization.Get("wrong_path");

            }
            // Création et stockage du modèle de sauvegarde
            var backup = new BackupModel(name, srcPath, destPath, type, DateTime.Now);

            backups[name] = backup;
            SaveBackups();

            long fileSize = GetSize(srcPath, destPath, type);

            Console.WriteLine( name, srcPath, destPath, fileSize, "Create");
             // Enregistrement du log de création de sauvegarde

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
                    // Mise à jour de l'état après la création de la sauvegarde
            }
            else
            {

                  totalfiles =1;
            }
            _stateService.TakeAndUpdateStates(name, srcPath, destPath, Localization.Get("backup_success"), type, totalfiles , fileSize, 0, 100);
                _stateService.StopTimer();


            return Localization.Get("backup_success");
        }
         // Récupère la liste de toutes les sauvegardes
        public List<BackupModel> GetAllBackups()
        {
            return new List<BackupModel>(backups.Values);
        }
        // Exécute les sauvegardes en parallèle et met à jour les logs et l'état
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
                        long totalfiles = CountFilesInDirectory(backup.SourcePath);
                        CopyDirectory(backup.SourcePath, backup.DestinationPath, backup.BackupType, name, backup.BackupType, fileSize);
                        _stateService.StartTimer(name, backup.SourcePath, backup.DestinationPath, Localization.Get("backup_run_success"), backup.BackupType, totalfiles, fileSize, 0, 100);


                    }
                    else
                    {                
                        long totalfiles = 1;    
                        Directory.CreateDirectory(Path.GetDirectoryName(backup.FullDestinationPath));
                        CopyFile(backup.SourcePath, backup.FullDestinationPath, backup.BackupType);
                        _stateService.StartTimer(name, backup.SourcePath, backup.DestinationPath, Localization.Get("backup_run_success"), backup.BackupType, totalfiles, fileSize, 0, 100);


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


                    _stateService.StopTimer();


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
         // Supprime les sauvegardes demandées et met à jour l'état
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
                                progression = 100;
                                Directory.Delete(directoryToDelete, true);
                                _stateService.TakeAndUpdateStates(name, backup.SourcePath, backup.DestinationPath, Localization.Get("directory_sucessfully_deleted"), backup.BackupType, totalfiles, fileSize, filesLeftToDo, progression);
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

                         _stateService.StopTimer();

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

     // Copie un répertoire et ses fichiers dans le répertoire de destination
        private void CopyDirectory(string sourceDir, string destDir, string backupType, string name, string type, long filesize)
        {
            var destDirWithSource = Path.Combine(destDir, Path.GetFileName(sourceDir));
            Directory.CreateDirectory(destDirWithSource);


            long copiedFiles = 0;


            foreach (var dir in Directory.GetDirectories(sourceDir, "*", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(dir.Replace(sourceDir, destDirWithSource));

            }

            
            long totalFiles = CountFilesInDirectory(sourceDir);

            foreach (var file in Directory.GetFiles(sourceDir, "*.*", SearchOption.AllDirectories))
            {

                long? filesLeftToDo = totalFiles; 
                string destinationFilePath = file.Replace(sourceDir, destDirWithSource);
                CopyFile(file, destinationFilePath, backupType);
                copiedFiles++;
                filesLeftToDo = totalFiles-copiedFiles;

                long progression = (long)((double)copiedFiles / totalFiles * 100); 

                _stateService.StartTimer(name, sourceDir, destDirWithSource, "Run en cours", type, totalFiles, filesize, filesLeftToDo, progression);
            }
            _stateService.StopTimer();


        }
            // Copie un fichier dans le répertoire de destination
        private void CopyFile(string sourceFile, string destFile, string backupType)
        {
            if (backupType.ToLower() == "full"|| backupType.ToLower() == "complÃ¨te" || !File.Exists(destFile))
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
        // Sauvegarde les sauvegardes dans le fichier de sauvegardes

        private void SaveBackups()
        {
            lock (_lock)
            {
                var json = JsonSerializer.Serialize(backups, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(BackupFilePath, json);
            }
        }
        // Charge les sauvegardes depuis le fichier de sauvegardes
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
        // Récupère la taille de la sauvegarde en fonction du type de sauvegarde
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
        // Récupère la taille du fichier en fonction du type de sauvegarde
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
        // Traite les fichiers dans un répertoire et retourne la taille totale et le nombre de fichiers traités
        private List<long> ProcessFilesInDirectory(DirectoryInfo currentDir, string destDirPath, string sourcePath, string destPath, string backupType)
        {
            
            List<long> result = new List<long> { 0, 0 };  // [0] pour la taille totale, [1] pour le nombre de fichiers traités
            long fileCount = 0; // Compteur pour le nombre de fichiers traités

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
        // Vérifie si le fichier source a été modifié
        private bool IsFileModified(string sourceFile, string destFile)
        {
            if (!File.Exists(destFile))
                return true;

            var sourceInfo = new FileInfo(sourceFile);
            var destInfo = new FileInfo(destFile);

            return sourceInfo.Length != destInfo.Length || sourceInfo.LastWriteTime > destInfo.LastWriteTime;
        }
        // Vérifie si le chemin est valide
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
        // Compte le nombre de fichiers dans un répertoire
        private int CountFilesInDirectory(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                throw new DirectoryNotFoundException($"Le répertoire {directoryPath} n'existe pas.");
            }
            return Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories).Length;
        }
}}