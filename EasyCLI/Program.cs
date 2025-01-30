using System;
using System.Collections.Generic;
using System.Diagnostics;
using Spectre.Console;
using EasyLib.ViewModels;
using EasySaveLog.Services;
using EasySaveLog.Models;

namespace EasyCLI
{
    class Program
    {
        static Stack<string> menuStack = new Stack<string>();

        static void Main(string[] args)
        {
            var dailyLogService = new DailyLogService(@"C:\Logs\Daily");
            var stateService = new StateService(@"C:\Logs\state.json");
            var viewModel = new BackupViewModel();
            menuStack.Push("Main Menu");

            AnsiConsole.MarkupLine(Localization.Get("choose_language"));
            var language = Console.ReadLine();
            if (!string.IsNullOrEmpty(language))
            {
                Localization.SetLanguage(language);
            }
            else
            {
                AnsiConsole.MarkupLine("[red]Language selection cannot be empty.[/]");
                return;
            }

            
            if (Environment.GetEnvironmentVariable("CI") == "true")
            {
                Console.WriteLine("Skipping interactive menu in CI environment.");
                return; 
            }


            while (true)
            {
                var choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title($"[bold green]{Localization.Get("menu_title")}[/]")
                        .PageSize(10)
                        .AddChoices(new[]
                        {
                            Localization.Get("create_backup"),
                            Localization.Get("list_backups"),
                            Localization.Get("run_backup"),
                            Localization.Get("run_all_backups"),
                            Localization.Get("delete_backup"),
                            Localization.Get("exit")
                        }));

                if (choice == Localization.Get("exit"))
                {
                    break;
                }
                switch (choice)
                {

                    case var _ when choice == Localization.Get("create_backup"):
                        CreateBackup(viewModel, dailyLogService);
                        break;
                     case var _ when choice == Localization.Get("list_backups"):
                        ListBackups(viewModel);
                        break;
                    case var _ when choice == Localization.Get("run_backup"):
                        RunBackupMenu(viewModel, dailyLogService);
                        break;
                    case var _ when choice == Localization.Get("run_all_backups"):
                        RunAllBackups(viewModel);
                        break;
                    case var _ when choice == Localization.Get("delete_backup"):
                        DeleteBackup(viewModel, dailyLogService);
                        break;
                    default:
                        AnsiConsole.MarkupLine($"[red]{Localization.Get("invalid_option")}[/]");
                        break;
                }
            }
        }
        


        static void CreateBackup(BackupViewModel viewModel, DailyLogService dailyLogService)
        {
            AnsiConsole.Clear();
            AnsiConsole.MarkupLine($"[bold]{Localization.Get("create_backup")}[/]");
            var name = AnsiConsole.Ask<string>(Localization.Get("enter_backup_name"));
            var srcPath = AnsiConsole.Ask<string>(Localization.Get("enter_source_path"));
            var destPath = AnsiConsole.Ask<string>(Localization.Get("enter_destination_path"));
            var type = AnsiConsole.Ask<string>(Localization.Get("enter_backup_type"));
    

            viewModel.CreateBackup(name, srcPath, destPath, type);

            long fileSize = dailyLogService.GetFileSize(srcPath);            

            dailyLogService.WriteLogEntry(new LogEntry
            {
                Timestamp = DateTime.Now,
                BackupName = name,
                SourcePath = srcPath,
                DestinationPath = destPath,
                FileSize = fileSize,
                Type="Create"           
                      
        });
            AnsiConsole.MarkupLine($"[bold blue]{viewModel.Status}[/]");

            AnsiConsole.Markup($"[bold yellow]{Localization.Get("press_backspace")}[/]");
            while (Console.ReadKey(true).Key != ConsoleKey.Backspace) { }
            menuStack.Pop();
        }

        static void ListBackups(BackupViewModel viewModel)
        {
            AnsiConsole.Clear();
            AnsiConsole.MarkupLine($"[bold]{Localization.Get("List_of_backup")}[/]");
            viewModel.ListBackups();
            AnsiConsole.MarkupLine($"[bold blue]{viewModel.Status}[/]");

            AnsiConsole.Markup($"[bold yellow]{Localization.Get("press_backspace")}[/]");
            while (Console.ReadKey(true).Key != ConsoleKey.Backspace) { }
            menuStack.Pop();
        }

        static void RunBackupMenu(BackupViewModel viewModel, DailyLogService dailyLogService)
        {
            var selectedBackup = viewModel.ShowBackup(Localization.Get("showRunbackup"));
            if (selectedBackup == null)
            {
                return;
            }
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            viewModel.RunBackup(selectedBackup.Name);
            stopwatch.Stop();
            long varTransfertime = stopwatch.ElapsedMilliseconds;


            dailyLogService.WriteLogEntry(new LogEntry
            {
                Timestamp = DateTime.Now,
                BackupName = selectedBackup.Name,
                SourcePath = selectedBackup.SourcePath,
                DestinationPath = selectedBackup.DestinationPath,
                FileSize = dailyLogService.GetFileSize(selectedBackup.SourcePath),
                Time = varTransfertime + "ms", 
                Type="Run" 
            });
            AnsiConsole.MarkupLine($"[bold blue]{viewModel.Status}[/]");

            AnsiConsole.Markup($"[bold yellow]{Localization.Get("press_backspace")}[/]");
            while (Console.ReadKey(true).Key != ConsoleKey.Backspace) { }
            menuStack.Pop();
        }
        static void RunAllBackups(BackupViewModel viewModel)
        {
            AnsiConsole.Clear();
            AnsiConsole.MarkupLine($"[bold]{Localization.Get("Running_all")}[/]");

            viewModel.RunAllBackups();
            AnsiConsole.MarkupLine($"[bold blue]{viewModel.Status}[/]");

            AnsiConsole.Markup($"[bold yellow]{Localization.Get("press_backspace")}[/]");
            while (Console.ReadKey(true).Key != ConsoleKey.Backspace) { }
            menuStack.Pop();
            
        }

        static void DeleteBackup(BackupViewModel viewModel, DailyLogService dailyLogService)
        {

            var selectedBackup = viewModel.ShowBackup(Localization.Get("showdeletebackup"));
            if (selectedBackup == null)
            {
                return;
            }
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            viewModel.DeleteBackup(selectedBackup.Name);
            stopwatch.Stop();
            long varTransfertime = stopwatch.ElapsedMilliseconds;


            dailyLogService.WriteLogEntry(new LogEntry
            {
                Timestamp = DateTime.Now,
                BackupName = selectedBackup.Name,
                SourcePath = selectedBackup.SourcePath,
                DestinationPath = selectedBackup.DestinationPath,
                FileSize = dailyLogService.GetFileSize(selectedBackup.SourcePath),
                Time = varTransfertime + "ms", 
                Type="Delete" 
            });
            AnsiConsole.MarkupLine($"[bold blue]{viewModel.Status}[/]");

            AnsiConsole.Markup($"[bold yellow]{Localization.Get("press_backspace")}[/]");
            while (Console.ReadKey(true).Key != ConsoleKey.Backspace) { }
            menuStack.Pop();
        }
    }
}