using System;
using System.Collections.Generic;
using System.Threading;
using Spectre.Console;
using EasyLib.Models;
using EasyLib.Services;       // Pour accéder à BackupService
using EasySaveLog.Services;   // Pour DailyLogService
// Assurez-vous que Localization est défini dans votre projet

namespace EasyLib.ViewModels
{
    public class BackupViewModel
    {
        private readonly BackupService _backupService;
        private readonly DailyLogService _dailyLogService;

        public string Status { get; private set; }

        public BackupViewModel(DailyLogService dailyLogService, BackupService backupService)
        {
            _dailyLogService = dailyLogService;
            _backupService = backupService;
        }

        
        public void CreateBackupFromUserInput()
        {
            Console.Clear();
            AnsiConsole.Clear();
            Thread.Sleep(100);
            AnsiConsole.MarkupLine($"[bold]{Localization.Get("create_backup")}[/]");

            var name = AnsiConsole.Ask<string>(Localization.Get("enter_backup_name"));
            var srcPath = AnsiConsole.Ask<string>(Localization.Get("enter_source_path"));
            var destPath = AnsiConsole.Ask<string>(Localization.Get("enter_destination_path"));
            var type = AnsiConsole.Ask<string>(Localization.Get("enter_backup_type"));

            
            Status = _backupService.CreateBackup(name, srcPath, destPath, type);
        }

        
        public string ListBackups()
        {
            var backups = _backupService.GetAllBackups();
            if (backups.Count == 0)
            {
                Status = Localization.Get("no_backups");
                return Status;
            }

            string listing = $"{Localization.Get("list_of_backup_jobs")}\n";
            foreach (var backup in backups)
            {
                listing += $"{backup.Name} - {backup.SourcePath} -> {backup.FullDestinationPath} ({backup.BackupType})\n";
            }
            Status = listing;
            return Status;
        }

        
        public void RunBackupFromUserSelection()
        {
            var backupsList = _backupService.GetAllBackups();
            if (backupsList.Count == 0)
            {
                AnsiConsole.MarkupLine($"[red]{Localization.Get("no_backups")}[/]");
                return;
            }

            var selected = AnsiConsole.Prompt(
                new MultiSelectionPrompt<string>()
                    .Title(Localization.Get("select_backup"))
                    .PageSize(10)
                    .InstructionsText($"[grey]{Localization.Get("use_space")}[/]")
                    .AddChoices(backupsList.ConvertAll(b => b.Name))
            );

            
            Status = _backupService.RunBackup(selected);
            AnsiConsole.MarkupLine($"[bold blue]Status: {Status}[/]");
        }

        
        public void DeleteBackupFromUserSelection()
        {
            var backupsList = _backupService.GetAllBackups();
            if (backupsList.Count == 0)
            {
                AnsiConsole.MarkupLine($"[red]{Localization.Get("no_backups")}[/]");
                return;
            }

            var selected = AnsiConsole.Prompt(
                new MultiSelectionPrompt<string>()
                    .Title(Localization.Get("select_backup"))
                    .PageSize(10)
                    .InstructionsText($"[grey]{Localization.Get("use_space")}[/]")
                    .AddChoices(backupsList.ConvertAll(b => b.Name))
            );

            
            Status = _backupService.DeleteBackup(selected);
            AnsiConsole.MarkupLine($"[bold blue]Status: {Status}[/]");
        }
    }
}