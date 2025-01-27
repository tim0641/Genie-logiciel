using System;
using System.IO;

namespace EasyLib.Models
{
    public class BackupModel
    {
        public string Name { get; }
        public string SourcePath { get; }
        public string DestinationPath { get; }
        public string BackupType { get; }
        public DateTime CreationTime { get; }
        public string FileName { get; }
        public string FullDestinationPath { get; }

        public bool IsDirectory { get; } 

        public BackupModel(string name, string sourcePath, string destinationPath, string backupType, DateTime creationTime)
        {
            Name = name;
            SourcePath = sourcePath;
            DestinationPath = destinationPath;
            BackupType = backupType;
            CreationTime = creationTime;
            IsDirectory = Directory.Exists(sourcePath); 

            // Initialisation des chemins basés sur le chemin source
            FileName = Path.GetFileName(sourcePath); // Récupérer le nom du fichier à partir du chemin source
            FullDestinationPath = Path.Combine(destinationPath, FileName); // Créer le chemin complet pour la destination
        }
    }
}