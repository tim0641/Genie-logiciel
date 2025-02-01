using System;
using System.Threading;
using Spectre.Console;
using EasyLib;
using EasyLib.ViewModels;
using EasyLib.Services;
using EasySaveLog.Services;


namespace EasyCLI
{
    class Program
    {
        static void Main(string[] args)
        {
            
            if (Environment.GetEnvironmentVariable("CI") == "true")
            {
                Console.WriteLine("Skipping interactive menu in CI environment.");
                return;
            }

            
            var dailyLogService = new DailyLogService(@"C:\Logs\Daily");
            var backupService = new BackupService();
            var viewModel = new BackupViewModel(dailyLogService, backupService);

            
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

            
            while (true)
            {
                Console.Clear();
                AnsiConsole.Clear();
                Thread.Sleep(100);

                var choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title($"[bold green]{Localization.Get("menu_title")}[/]")
                        .PageSize(10)
                        .AddChoices(new[]
                        {
                            Localization.Get("create_backup"),
                            Localization.Get("list_backups"),
                            Localization.Get("run_backup"),
                            Localization.Get("delete_backup"),
                            Localization.Get("exit")
                        }));

                if (choice == Localization.Get("exit"))
                    break;
                switch (choice)
                {
                    case var _ when choice == Localization.Get("create_backup"):
                        viewModel.CreateBackupFromUserInput();
                        AnsiConsole.MarkupLine($"[bold blue]{viewModel.Status}[/]");
                        break;
                    case var _ when choice == Localization.Get("list_backups"):
                        
                        string listResult = viewModel.ListBackups();
                        AnsiConsole.MarkupLine($"[bold blue]{listResult}[/]");
                        break;
                    case var _ when choice == Localization.Get("run_backup"):
                        viewModel.RunBackupFromUserSelection();
                        break;
                    case var _ when choice == Localization.Get("delete_backup"):
                        viewModel.DeleteBackupFromUserSelection();
                        break;
                    default:
                        AnsiConsole.MarkupLine($"[red]{Localization.Get("invalid_option")}[/]");
                        break;
                }

                WaitForBackspace();
            }
        }

        static void WaitForBackspace()
        {
            AnsiConsole.Markup("[bold yellow]"+Localization.Get("press_backspace")+"[/]");
            while (Console.ReadKey(true).Key != ConsoleKey.Backspace) { }
        }
    }
}