using System;
using System.IO;
using System.Windows;
using System.Text.Json;
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
public static string GetCryptoSoftExecutablePath()
{
    // Récupère le répertoire d'exécution de l'application
    string baseDir = AppContext.BaseDirectory;

    // Construit le chemin vers CryptoSoft.exe
    string cryptoExePath = Path.Combine(baseDir, "CryptoSoft", "CryptoSoft.exe");

    // Vérification que le fichier existe
    if (!File.Exists(cryptoExePath))
    {
        MessageBox.Show("Le fichier CryptoSoft.exe est introuvable à l'emplacement : " + cryptoExePath, 
                        "Erreur de chemin", 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Error);
        return null; // Retourne null pour indiquer que le fichier n'existe pas
    }

    return cryptoExePath;
}


}


public static class ConfigHelper
{
    public static string GetCryptoSoftPath()
    {
        string configPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
         MessageBox.Show("Recherche de appsettings.json à : " + configPath, "Debug", MessageBoxButton.OK, MessageBoxImage.Information);

        if (!File.Exists(configPath))
        {
            MessageBox.Show("Le fichier de configuration est introuvable.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            return null;
        }

        string json = File.ReadAllText(configPath);
        var config = JsonSerializer.Deserialize<Dictionary<string, string>>(json);

        if (config != null && config.ContainsKey("CryptoSoftPath") && File.Exists(config["CryptoSoftPath"]))
        {
            return config["CryptoSoftPath"];
        }
        else
        {
            MessageBox.Show("Le chemin de CryptoSoft.exe est incorrect ou inexistant.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            return null;
        }
    }
}




