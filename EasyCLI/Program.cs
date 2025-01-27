using System;
using System.Collections.Generic;
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

            while (menuStack.Count > 0)
            {
                string currentMenu = menuStack.Peek();

                switch (currentMenu)
                {
                    case "Main Menu":
                        ShowMainMenu(viewModel);
                        break;
                    case "Create Backup":
                        CreateBackup(viewModel, dailyLogService);
                        break;
                    case "List Backups":
                        ListBackups(viewModel);
                        break;
                    case "Run Backup":
                        RunBackupMenu(viewModel, dailyLogService);
                        break;
                    case "Delete Backup":
                        DeleteBackup(viewModel, dailyLogService);
                        break;
                    default:
                        menuStack.Pop();
                        break;
                }
            }
        }

        static void ShowMainMenu(BackupViewModel viewModel)
        {
            AnsiConsole.Clear();
            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold green]Main Menu[/]")
                    .PageSize(10)
                    .AddChoices(new[] {
                        "Create Backup", "List Backups", "Run Backup", "Delete Backup", "Exit"
                    }));

            if (choice == "Exit")
            {
                menuStack.Pop();
            }
            else
            {
                menuStack.Push(choice);
            }
        }

        static void CreateBackup(BackupViewModel viewModel, DailyLogService dailyLogService)
        {
            AnsiConsole.Clear();
            AnsiConsole.MarkupLine("[bold]Create Backup[/]");
            var name = AnsiConsole.Ask<string>("Enter [green]backup name[/]:");
            var srcPath = AnsiConsole.Ask<string>("Enter [green]source path[/]:");
            var destPath = AnsiConsole.Ask<string>("Enter [green]destination path[/]:");
            var type = AnsiConsole.Ask<string>("Enter [green]backup type[/] (Full/Differential):");

            viewModel.CreateBackup(name, srcPath, destPath, type);
            dailyLogService.WriteLogEntry(new LogEntry
            {
                Timestamp = DateTime.Now,
                BackupName = name,
                SourcePath = srcPath,
                DestinationPath = destPath,
                FileSize = 0,           
                TransferTime = 0        
        });
            AnsiConsole.MarkupLine($"[bold blue]{viewModel.Status}[/]");

            AnsiConsole.Markup("[bold yellow]Press Backspace to return to the main menu...[/]");
            while (Console.ReadKey(true).Key != ConsoleKey.Backspace) { }
            menuStack.Pop();
        }

        static void ListBackups(BackupViewModel viewModel)
        {
            AnsiConsole.Clear();
            AnsiConsole.MarkupLine("[bold]List of Backups[/]");
            viewModel.ListBackups();
            AnsiConsole.MarkupLine($"[bold blue]{viewModel.Status}[/]");

            AnsiConsole.Markup("[bold yellow]Press Backspace to return to the main menu...[/]");
            while (Console.ReadKey(true).Key != ConsoleKey.Backspace) { }
            menuStack.Pop();
        }

        static void RunBackupMenu(BackupViewModel viewModel, DailyLogService dailyLogService)
        {
            AnsiConsole.Clear();
            AnsiConsole.MarkupLine("[bold]Run Backup[/]");

            var backups = viewModel.GetBackupList();
            if (backups.Count == 0)
            {
                AnsiConsole.MarkupLine("[red]No backup jobs available.[/]");
                AnsiConsole.Markup("[bold yellow]Press Backspace to return to the main menu...[/]");
                while (Console.ReadKey(true).Key != ConsoleKey.Backspace) { }
                menuStack.Pop();
                return;
            }

            var selectedBackup = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select a backup to run:")
                    .PageSize(10)
                    .AddChoices(backups.ConvertAll(b => b.Name)));

            viewModel.RunBackup(selectedBackup);
            dailyLogService.WriteLogEntry(new LogEntry
            {
                Timestamp = DateTime.Now,
                BackupName = selectedBackup,
                SourcePath = "N/A",
                DestinationPath = "N/A",
                FileSize = 0,
                TransferTime = 0
            });
            AnsiConsole.MarkupLine($"[bold blue]{viewModel.Status}[/]");

            AnsiConsole.Markup("[bold yellow]Press Backspace to return to the main menu...[/]");
            while (Console.ReadKey(true).Key != ConsoleKey.Backspace) { }
            menuStack.Pop();
        }

        static void DeleteBackup(BackupViewModel viewModel, DailyLogService dailyLogService)
        {
            AnsiConsole.Clear();
            AnsiConsole.MarkupLine("[bold]Delete Backup[/]");
            var name = AnsiConsole.Ask<string>("Enter [green]backup name[/] to delete:");

            viewModel.DeleteBackup(name);
            dailyLogService.WriteLogEntry(new LogEntry
            {
                Timestamp = DateTime.Now,
                BackupName = name,
                SourcePath = "N/A",
                DestinationPath = "N/A",
                FileSize = 0,
                TransferTime = 0
            });
            AnsiConsole.MarkupLine($"[bold blue]{viewModel.Status}[/]");

            AnsiConsole.Markup("[bold yellow]Press Backspace to return to the main menu...[/]");
            while (Console.ReadKey(true).Key != ConsoleKey.Backspace) { }
            menuStack.Pop();
        }
    }
}