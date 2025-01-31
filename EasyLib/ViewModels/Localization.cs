using System.Collections.Generic;

namespace EasyLib
{
    public static class Localization
    {
        private static string currentLanguage = "fr"; // Langue par défaut
        private static readonly string defaultLanguage = "en"; // Langue de secours

        private static readonly Dictionary<string, Dictionary<string, string>> translations = new Dictionary<string, Dictionary<string, string>>
        {
            { "en", new Dictionary<string, string>
                {
                    { "menu_title", "Main Menu" },
                    { "create_backup", "Create Backup" },
                    { "list_backups", "List Backups" },
                    { "run_backup", "Run Backup" },
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
                    { "backup_exists", "Backup job already exists." },
                    { "no_backups", "No backups available." },
                    { "backup_deleted", "Backup job deleted successfully." },
                    { "list_of_backups", "List of Backups :" },
                    { "Select_run", "Select backups to run:" },
                    { "Use_space", "Use space to select multiple, enter to confirm" },
                    { "no_backup_select", "No backups selected." },
                    { "backup_completed", "- Backup completed in" },
                    { "backup_completed_dele", "- Backup deleted in" },
                    { "Select_del", "Select backup to delete" },



                }
            },
            { "fr", new Dictionary<string, string>
                {
                    { "menu_title", "Menu Principal" },
                    { "create_backup", "Créer une sauvegarde" },
                    { "list_backups", "Lister les sauvegardes" },
                    { "run_backup", "Exécuter la sauvegarde" },
                    { "delete_backup", "Supprimer la sauvegarde" },
                    { "exit", "Quitter" },
                    { "choose_language", "Choisissez une langue : (EN/FR)" },
                    { "invalid_option", "Option invalide. Veuillez réessayer." },
                    { "press_backspace", "Appuyez sur Retour arrière pour revenir au menu principal..." },
                    { "enter_backup_name", "Entrez le nom de la sauvegarde :" },
                    { "enter_source_path", "Entrez le chemin source :" },
                    { "enter_destination_path", "Entrez le chemin de destination :" },
                    { "enter_backup_type", "Entrez le type de sauvegarde (Complete/Différentielle) :" },
                    { "backup_success", "Tâche de sauvegarde créée avec succès." },
                    { "backup_exists", "La tâche de sauvegarde existe déjà." },
                    { "no_backups", "Aucune sauvegarde disponible." },
                    { "backup_deleted", "Tâche de sauvegarde supprimée avec succès." },
                    { "list_of_backups", "Liste des Backups :" },
                    { "Use_space", "Utiliser espace pour séléctionné plusieurs backup" },
                    { "Select_run", "Selectionné la Backup à executé " },
                    { "Select_del", "Selectionné la Backup à supprimé " },
                    { "no_backup_select", "Pas de backup sélectionné" },
                    { "backup_completed", "Backup effectué en" },
                    { "backup_completed_dele", "- Backup supprimé en" }

                }
            }
        };

        public static void SetLanguage(string lang)
        {
            lang = lang.ToLower();
            if (translations.ContainsKey(lang))
            {
                currentLanguage = lang;
            }
            else
            {
                currentLanguage = defaultLanguage;
            }
        }

        public static string Get(string key)
        {
            if (translations[currentLanguage].ContainsKey(key))
            {
                return translations[currentLanguage][key];
            }
            return translations[defaultLanguage].ContainsKey(key) ? translations[defaultLanguage][key] : key;
        }
    }
}