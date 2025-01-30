using System.Collections.Generic;

namespace EasyCLI
{
    public static class Localization
    {
        private static string currentLanguage = "fr"; // Langue par défaut
        private static Dictionary<string, Dictionary<string, string>> translations = new Dictionary<string, Dictionary<string, string>>
        {
            { "en", new Dictionary<string, string>
                {
                    { "menu_title", "Main Menu" },
                    { "create_backup", "Create Backup" },
                    { "list_backups", "List Backups" },
                    { "run_backup", "Run Backup" },
                    { "run_all_backups", "Run All Backups" },
                    { "delete_backup", "Delete Backup" },
                    { "exit", "Exit" },
                    { "choose_language", "Choose a language: (EN/FR)" },
                    { "invalid_option", "Invalid option. Please try again." },
                    { "press_backspace", "Press Backspace to return to the main menu..." },
                    { "enter_backup_name", "Enter backup name:" },
                    { "enter_source_path", "Enter source path:" },
                    { "enter_destination_path", "Enter destination path:" },
                    { "enter_backup_type", "Enter backup type (Full/Differential):" },
                    { "backup_success", "Backup job created successfully." },
                    { "no_backups", "No backup jobs available." },
                    { "backup_deleted", "Backup job deleted successfully." },
                    { "running_all_backups", "Running all backups..." },
                    { "List_of_backup", "List of Backups" },
                    { "showRunbackup", "Select backup to run" },
                    { "showdeletebackup", "Select backup to delete" },
                    { "Running_all", "Running All Backup" }


                }
            },
            { "fr", new Dictionary<string, string>
                {
                    { "menu_title", "Menu Principal" },
                    { "create_backup", "Créer une Sauvegarde" },
                    { "list_backups", "Lister les Sauvegardes" },
                    { "run_backup", "Exécuter une Sauvegarde" },
                    { "run_all_backups", "Exécuter Toutes les Sauvegardes" },
                    { "delete_backup", "Supprimer une Sauvegarde" },
                    { "exit", "Quitter" },
                    { "choose_language", "Choisissez une langue : (EN/FR)" },
                    { "invalid_option", "Option invalide. Veuillez réessayer." },
                    { "press_backspace", "Appuyez sur Retour pour revenir au menu principal..." },
                    { "enter_backup_name", "Entrez le nom de la sauvegarde :" },
                    { "enter_source_path", "Entrez le chemin source :" },
                    { "enter_destination_path", "Entrez le chemin de destination :" },
                    { "enter_backup_type", "Entrez le type de sauvegarde (Full/Differential) :" },
                    { "backup_success", "Travail de sauvegarde créé avec succès." },
                    { "no_backups", "Aucune sauvegarde disponible." },
                    { "backup_deleted", "Sauvegarde supprimée avec succès." },
                    { "running_all_backups", "Exécution de toutes les sauvegardes..." },
                    { "List_of_backup", "Liste des Backups" },
                    { "showdeletebackup", "Selectionné la backup à supprimer" },
                    { "showRunbackup", "Selectionné la backup à effectuer" },
                    { "Running_all", "Effectuer toute les Backups" }

                }
            }
        };

        public static void SetLanguage(string lang)
        {
            if (translations.ContainsKey(lang.ToLower()))
            {
                currentLanguage = lang.ToLower();
            }
        }

        public static string Get(string key)
        {
            return translations[currentLanguage].ContainsKey(key) ? translations[currentLanguage][key] : key;
        }
    }
}