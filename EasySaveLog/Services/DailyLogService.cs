using System.Text.Json;
using EasySaveLog.Models;

namespace EasySaveLog.Services
{
    public class DailyLogService
    {
        private readonly string _logsDirectory;

        public DailyLogService(string logsDirectory)
        {
            _logsDirectory = logsDirectory;
            Directory.CreateDirectory(_logsDirectory);
        }

        public void WriteLogEntry(LogEntry entry)
        {
            
            string fileName = DateTime.Now.ToString("yyyy-MM-dd") + ".json";
            string filePath = Path.Combine(_logsDirectory, fileName);

           
            string json = JsonSerializer.Serialize(entry, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            
            File.AppendAllText(filePath, json + Environment.NewLine);
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

    if (backupType.ToLower() == "full" || !File.Exists(destinationFile))
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
    Console.WriteLine("On entre dans directory size");
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
    Console.WriteLine("On entre dans processFile");
    long size = 0;
    foreach (var file in currentDir.GetFiles())
    {
        string relativeFilePath = Path.GetRelativePath(sourcePath, file.FullName);
        string correspondingDestFile = Path.Combine(destPath, Path.GetFileName(sourcePath), relativeFilePath);
        Console.WriteLine(correspondingDestFile);

        if (backupType.ToLower() == "full" || !File.Exists(correspondingDestFile))
        {
            size += file.Length;
        }
        else
        {
            if (IsFileModified(file.FullName, correspondingDestFile))
            {
                Console.WriteLine("Fichier modifié");
                size += file.Length;
            }
        }
    }
    return size;
}

private bool IsFileModified(string sourceFile, string destFile)
{
    Console.WriteLine("On entre dans isFileModified");
    FileInfo sourceInfo = new FileInfo(sourceFile);
    FileInfo destInfo = new FileInfo(destFile);

    bool a = !File.Exists(destFile) || sourceInfo.Length != destInfo.Length || sourceInfo.LastWriteTime > destInfo.LastWriteTime;
    Console.WriteLine(sourceInfo.Name+"------------------->"+destInfo.Name);
    Console.WriteLine(a);
    return a;

}
public void FlushLogs()
{
    Console.Out.Flush();
}

    }
}


