using System;
using System.Threading;
using Spectre.Console;
using EasyLib;
using EasyLib.ViewModels;
using EasyLib.Services;
using EasySaveLog.Services;
using EasySaveLog.Models;
using System.Diagnostics.Metrics;
using System.Diagnostics;



namespace EasyCLI
{

    
    class Program
    {
        static Timer timer;
        static string dernierChoix = "Aucun choix";
            int counter = 1;

        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                ProcessCommandLine(args);
            }
            else
            {
            
            if (Environment.GetEnvironmentVariable("CI") == "true")
            {
                Console.WriteLine("Skipping interactive menu in CI environment.");
                return;
            }


            var dailyLogService = new DailyLogService(@"C:\Logs\Daily");
            var backupService = new BackupService();
            var stateService = new StateService(@"C:\Logs\States\Daily");
            var viewModel = new BackupViewModel(dailyLogService, backupService,stateService);
        
 
            AnsiConsole.MarkupLine(Localization.Get("choose_language"));
            var language = Console.ReadLine();
            if (!string.IsNullOrEmpty(language))
            {
                Console.Clear();
                AnsiConsole.Clear();
                Localization.SetLanguage(language);

            }
            else
            {   

                AnsiConsole.MarkupLine("[red]Language selection cannot be empty.[/]");
                return;
            }

            stateService.StartTimer("", "", "", Localization.Get("menu_title"), "", 0, 0, 0, 0);


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
                        stateService.StopTimer();
                        stateService.StartTimer("", "", "", Localization.Get("create_backup"), "", 0, 0, 0, 0);
                        viewModel.CreateBackupFromUserInput();
                        AnsiConsole.MarkupLine($"[bold blue]{viewModel.Status}[/]");
                        break;
                    case var _ when choice == Localization.Get("list_backups"):
                        stateService.StopTimer();
                        stateService.StartTimer("", "", "", Localization.Get("list_backups"), "", 0, 0, 0, 0);
                        string listResult = viewModel.ListBackups();
                        AnsiConsole.MarkupLine($"[bold blue]{listResult}[/]");
                        break;
                    case var _ when choice == Localization.Get("run_backup"):
                        stateService.StopTimer();
                        stateService.StartTimer("", "", "", Localization.Get("run_backup"), "", 0, 0, 0, 0);
                        viewModel.RunBackupFromUserSelection();
                        break;
            
                    case var _ when choice == Localization.Get("delete_backup"):
                        stateService.StopTimer();
                        stateService.StartTimer("", "", "", Localization.Get("delete_backup"), "", 0, 0, 0, 0);
                        viewModel.DeleteBackupFromUserSelection();
                        break;
                    default:
                        AnsiConsole.MarkupLine($"[red]{Localization.Get("invalid_option")}[/]");
                        break;
                }


            WaitForBackspace(stateService);
            
            stateService.StartTimer("", "", "", Localization.Get("menu_title"), "", 0, 0, 0, 0);



            }
        }

        static void WaitForBackspace(StateService stateService)
        {                  
            stateService.StopTimer();
            AnsiConsole.Markup("[bold yellow]"+Localization.Get("press_backspace")+"[/]");
            while (Console.ReadKey(true).Key != ConsoleKey.Backspace) { }

        }

        static void ProcessCommandLine(string[] args)
        {
            // create <name> <srcPath> <destPath> <type>
            // list
            // run <name1> [<name2> ...]
            // delete <name1> [<name2> ...]
            var dailyLogService = new DailyLogService(@"C:\Logs\Daily");
            var backupService = new BackupService();
            var stateService = new StateService(@"C:\Logs\States\Daily");
            var viewModel = new BackupViewModel(dailyLogService, backupService,stateService);

            string command = args[0].ToLower();

            switch (command)
            {
                case "create":
                    if (args.Length < 5)
                    {
                        Console.WriteLine("Usage: create <name> <srcPath> <destPath> <type>");
                        return;
                    }
                    string name = args[1];
                    string srcPath = args[2];
                    string destPath = args[3];
                    string type = args[4];
                    backupService.CreateBackup(name, srcPath, destPath, type);
                    Console.WriteLine("Status: " + viewModel.Status);
                    break;

                case "list":
                    Console.WriteLine(viewModel.ListBackups());
                    break;

                case "run":
                    if (args.Length < 2)
                    {
                        Console.WriteLine("Usage: run <name1> [<name2> ...]");
                        return;
                    }
                    var runNames = new List<string>(args[1..]);
                    backupService.RunBackup(runNames);
                    Console.WriteLine("Status: " + viewModel.Status);
                    break;

                case "delete":
                    if (args.Length < 2)
                    {
                        Console.WriteLine("Usage: delete <name1> [<name2> ...]");
                        return;
                    }
                    var deleteNames = new List<string>(args[1..]);
                    backupService.DeleteBackup(deleteNames);
                    Console.WriteLine("Status: " + viewModel.Status);
                    break;

                default:
                    Console.WriteLine("Unknown command.");
                    break;
            }
        }
        }
    }
}