using System.Collections.Generic;


namespace EasyLib
{
    public static class Localization
    {
        private static string currentLanguage = "fr"; // Langue par défaut
        public static Dictionary<string, Dictionary<string, string>> translations = new Dictionary<string, Dictionary<string, string>>
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
                    { "no_backups", "No backups available." },
                    { "no_backupsjob", "No backup jobs available." },
                    { "backup_deleted", "Backup job deleted successfully." },
                    { "backup_run_success", "Backup completed successfully." },
                    { "backup_error", "Error during backup:" },
                    { "backup_exists", "Backup job already exists." },
                    { "backup_not_found", "Backup job not found." },
                    { "running_all_backups", "Running all backups..." },
                    { "list_of_backup_jobs", "List of backup jobs:" },
                    { "list_of_backups", "List of Backups" },
                    { "show_run_backup", "Select backup to run" },
                    { "show_delete_backup", "Select backup to delete" },
                    { "running_all", "Running All Backups" },
                    { "error_delete", "Error deleting backup file:" },
                    { "directory_delete", "Backup job and associated directory deleted successfully." },
                    { "directory_delete_not_found", "Backup job deleted, but the associated directory was not found." },
                    { "file_delete", "Backup job and associated file deleted successfully." },
                    { "file_delete_not_found", "Backup job deleted, but the associated file was not found." }

                }
            },
            { "fr", new Dictionary<string, string>
                {
                    { "list_backups", "Lister les sauvegardes" },
                    { "run_backup", "Exécuter la sauvegarde" },
                    { "run_all_backups", "Exécuter toutes les sauvegardes" },
                    { "delete_backup", "Supprimer la sauvegarde" },
                    { "exit", "Quitter" },
                    { "choose_language", "Choisissez une langue : (EN/FR)" },
                    { "invalid_option", "Option invalide. Veuillez réessayer." },
                    { "press_backspace", "Appuyez sur Retour arrière pour revenir au menu principal..." },
                    { "enter_backup_name", "Entrez le nom de la sauvegarde :" },
                    { "enter_source_path", "Entrez le chemin source :" },
                    { "enter_destination_path", "Entrez le chemin de destination :" },
                    { "enter_backup_type", "Entrez le type de sauvegarde (Complète/Différentielle) :" },
                    { "backup_success", "Tâche de sauvegarde créée avec succès." },
                    { "no_backups", "Aucune sauvegarde disponible." },
                    { "no_backupsjob", "Aucune tâche de sauvegarde disponible." },
                    { "backup_deleted", "Tâche de sauvegarde supprimée avec succès." },
                    { "backup_run_success", "Sauvegarde terminée avec succès." },
                    { "backup_error", "Erreur lors de la sauvegarde :" },
                    { "backup_exists", "La tâche de sauvegarde existe déjà." },
                    { "backup_not_found", "Tâche de sauvegarde non trouvée." },
                    { "running_all_backups", "Exécution de toutes les sauvegardes..." },
                    { "list_of_backup_jobs", "Liste des tâches de sauvegarde :" },
                    { "list_of_backups", "Liste des sauvegardes" },
                    { "show_run_backup", "Sélectionnez la sauvegarde à exécuter" },
                    { "show_delete_backup", "Sélectionnez la sauvegarde à supprimer" },
                    { "running_all", "Exécution de toutes les sauvegardes" },
                    { "error_delete", "Erreur lors de la suppression du fichier de sauvegarde :" },
                    { "directory_delete", "Tâche de sauvegarde et répertoire associé supprimés avec succès." },
                    { "directory_delete_not_found", "Tâche de sauvegarde supprimée, mais le répertoire associé n'a pas été trouvé." },
                    { "file_delete", "Tâche de sauvegarde et fichier associé supprimés avec succès." },
                    { "file_delete_not_found", "Tâche de sauvegarde supprimée, mais le fichier associé n'a pas été trouvé." }

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