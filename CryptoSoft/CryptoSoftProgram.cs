namespace CryptoSoft;

public static class CryptoSoftProgram
{
    public static void Main(string[] args)
    {
        try
        {
            Console.WriteLine($"Arguments reçus : {string.Join(" ", args)}");

            if (args.Length < 3)
            {
                Console.WriteLine("Erreur : Pas assez d'arguments !");
                Console.WriteLine("Usage : dotnet run <fichier> <clé> [--encrypt | --decrypt]");
                Environment.Exit(-1);
            }

            string mode = args[2].ToLower();
            bool encryptMode = mode == "--encrypt";
            bool decryptMode = mode == "--decrypt";

            if (!encryptMode && !decryptMode)
            {
                Console.WriteLine("Erreur : Le mode doit être --encrypt ou --decrypt !");
                Environment.Exit(-2);
            }

            var fileManager = new FileManager(args[0], args[1]);
            int elapsedTime = fileManager.TransformFile();

            Console.WriteLine(encryptMode ? "Chiffrement terminé !" : "Déchiffrement terminé !");
            Environment.Exit(elapsedTime);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Erreur dans CryptoSoft : {e.Message}");
            Environment.Exit(-99);
        }
    }
}
