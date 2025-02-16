using System.Diagnostics;
using System.Text;

namespace CryptoSoft;

/// <summary>
/// File manager class
/// This class is used to encrypt and decrypt files
/// </summary>
public class FileManager(string path, string key)
{
    private string FilePath { get; } = path;
    private string Key { get; } = key;

    /// <summary>
    /// check if the file exists
    /// </summary>
    private bool CheckFile()
    {
        if (File.Exists(FilePath))
            return true;

        Console.WriteLine("File not found.");
        Thread.Sleep(1000);
        return false;
    }

    /// <summary>
    /// Encrypts the file with xor encryption
    /// </summary>
public int TransformFile()
{
    if (!CheckFile()) return -1;
    Stopwatch stopwatch = Stopwatch.StartNew();

    Console.WriteLine($"Lecture du fichier : {FilePath}");
    var fileBytes = File.ReadAllBytes(FilePath);
    var keyBytes = ConvertToByte(Key);
    
    Console.WriteLine($"Taille du fichier avant transformation : {fileBytes.Length} octets");

    bool alreadyEncrypted = IsFileEncrypted(fileBytes);
    Console.WriteLine(alreadyEncrypted ? "Le fichier semble déjà chiffré." : "Le fichier semble en clair.");

    fileBytes = XorMethod(fileBytes, keyBytes);
    
    File.WriteAllBytes(FilePath, fileBytes);
    stopwatch.Stop();

    Console.WriteLine($"Transformation terminée en {stopwatch.ElapsedMilliseconds} ms.");
    return (int)stopwatch.ElapsedMilliseconds;
}


    /// <summary>
    /// Convert a string in byte array
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    private static byte[] ConvertToByte(string text)
    {
        return Encoding.UTF8.GetBytes(text);
    }

    /// <summary>
    /// </summary>
    /// <param name="fileBytes">Bytes of the file to convert</param>
    /// <param name="keyBytes">Key to use</param>
    /// <returns>Bytes of the encrypted file</returns>
    private static byte[] XorMethod(IReadOnlyList<byte> fileBytes, IReadOnlyList<byte> keyBytes)
    {
        var result = new byte[fileBytes.Count];
        for (var i = 0; i < fileBytes.Count; i++)
        {
            result[i] = (byte)(fileBytes[i] ^ keyBytes[i % keyBytes.Count]);
        }

        return result;
    }

    private bool IsFileEncrypted(byte[] fileBytes)
{
    if (fileBytes.Length < 10) return false; // Trop court pour être analysé

    // Vérifie si les premiers octets du fichier semblent aléatoires
    int entropyCount = 0;
    for (int i = 1; i < 10; i++)
    {
        if (fileBytes[i] != fileBytes[i - 1])
            entropyCount++;
    }
    
    return entropyCount > 5; // Si plus de 5 octets différents, on suppose un chiffrement
}
}
