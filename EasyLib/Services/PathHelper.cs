using System;
using System.IO;
using System.Windows;
public static class PathHelper
{
    /// <summary>
    /// Renvoie le répertoire pour les logs, par exemple %APPDATA%\EasySave\Logs\Daily
    /// </summary>
    public static string GetLogsDirectory() =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EasySave", "Logs", "Daily");

    /// <summary>
    /// Renvoie le répertoire pour les états, par exemple %APPDATA%\EasySave\Logs\States\Daily
    /// </summary>
    public static string GetStatesDirectory() =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EasySave", "Logs", "States", "Daily");

    /// <summary>
    /// Renvoie le chemin du projet CryptoSoft, ici stocké dans un sous-dossier relatif à l’exécutable.
    /// </summary>
    public static string GetCryptoSoftProjectPath()
{
string baseDir = AppDomain.CurrentDomain.BaseDirectory;
        // Construit le chemin relatif vers le dossier Cryptosoft situé dans le parent.
        string cryptoPath = Path.GetFullPath(Path.Combine(
        baseDir, 
        "..", 
        "..", 
        "..", 
        "..", 
        "Cryptosoft", 
        "CryptoSoft.csproj"
    ));

        // Vérification (MessageBox pour déboguer)
        if (!File.Exists(cryptoPath))
        {
            MessageBox.Show("Le fichier CryptoSoft.csproj est introuvable à l'emplacement : " + cryptoPath, 
                            "Erreur de chemin", 
                            MessageBoxButton.OK, 
                            MessageBoxImage.Error);
        }

        return cryptoPath;
}

}
