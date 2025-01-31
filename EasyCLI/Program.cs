﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using Spectre.Console;
using EasyLib;
using EasyLib.ViewModels;
using EasySaveLog.Services;
using EasySaveLog.Models;

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
            var stateService = new StateService(@"C:\Logs\state.json");
            var viewModel = new BackupViewModel();

            // Sélection de la langue
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

            // Boucle du menu principal
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
                    case var _ when choice == Localization.Get("delete_backup"):
                        DeleteBackupMenu(viewModel, dailyLogService);
                        break;
                    default:
                        AnsiConsole.MarkupLine($"[red]{Localization.Get("invalid_option")}[/]");
                        break;
                }
            }
        }

        static void CreateBackup(BackupViewModel viewModel, DailyLogService dailyLogService)
        {

            Console.Clear();
            AnsiConsole.Clear();
            Thread.Sleep(100);
            AnsiConsole.MarkupLine($"[bold]{Localization.Get("create_backup")}[/]");
            var name = AnsiConsole.Ask<string>(Localization.Get("enter_backup_name"));
            var srcPath = AnsiConsole.Ask<string>(Localization.Get("enter_source_path"));
            var destPath = AnsiConsole.Ask<string>(Localization.Get("enter_destination_path"));
            var type = AnsiConsole.Ask<string>(Localization.Get("enter_backup_type"));

            viewModel.CreateBackup(name, srcPath, destPath, type);
            long fileSize = dailyLogService.GetSize(srcPath, destPath, type);

            dailyLogService.WriteLogEntry(new LogEntry
            {
                Timestamp = DateTime.Now,
                BackupName = name,
                SourcePath = srcPath,
                DestinationPath = destPath,
                FileSize = fileSize,
                Type = "Create"
            });

            AnsiConsole.MarkupLine($"[bold blue]{viewModel.Status}[/]");
            WaitForBackspace();
        }

        static void ListBackups(BackupViewModel viewModel)
        {
            Console.Clear();
            AnsiConsole.Clear();
            Thread.Sleep(100);
            viewModel.ListBackups();
            AnsiConsole.MarkupLine($"[bold blue]{viewModel.Status}[/]");
            WaitForBackspace();
        }

        static void RunBackupMenu(BackupViewModel viewModel, DailyLogService dailyLogService)
        {
            Console.Clear();
            AnsiConsole.Clear();
            Thread.Sleep(100);
            AnsiConsole.MarkupLine($"[bold]{Localization.Get("run_backup")}[/]");

            var backups = viewModel.GetBackupList();
            if (backups.Count == 0)
            {
                AnsiConsole.MarkupLine($"[red]{Localization.Get("no_backups")}[/]");
                WaitForBackspace();
                return;
            }

            var selectedBackups = AnsiConsole.Prompt(
                new MultiSelectionPrompt<string>()
                    .Title(Localization.Get("Select_run"))
                    .PageSize(10)
                    .InstructionsText($"[grey]{Localization.Get("Use_space")}[/]")
                    .AddChoices(backups.ConvertAll(b => b.Name)));

            if (selectedBackups.Count == 0)
            {
                AnsiConsole.MarkupLine($"[yellow]{Localization.Get("no_backup_select")}[/]");
                return;
            }

            List<string> allStatuses = new List<string>();

            Parallel.ForEach(selectedBackups, (backupName) =>
            {
                var selectedBackup = viewModel.GetBackupByName(backupName);
                if (selectedBackup != null)
                {
                    var stopwatch = new Stopwatch();
                    stopwatch.Start();
                    viewModel.RunBackup(new List<string> { selectedBackup.Name });
                    stopwatch.Stop();

                    long transferTime = stopwatch.ElapsedMilliseconds;

                    dailyLogService.WriteLogEntry(new LogEntry
                    {
                        Timestamp = DateTime.Now,
                        BackupName = selectedBackup.Name,
                        SourcePath = selectedBackup.SourcePath,
                        DestinationPath = selectedBackup.DestinationPath,
                        FileSize = dailyLogService.GetSize(selectedBackup.SourcePath, selectedBackup.DestinationPath, selectedBackup.Name),
                        Time = transferTime + "ms",
                        Type = "Run"
                    });

                    lock (allStatuses)
                    {
                        allStatuses.Add($"{selectedBackup.Name} {Localization.Get("backup_completed")} {transferTime} ms.");
                    }
                }
            });

            foreach (string status in allStatuses)
            {
                AnsiConsole.MarkupLine($"[bold blue]{status}[/]");
            }

            WaitForBackspace();
        }

        static void DeleteBackupMenu(BackupViewModel viewModel, DailyLogService dailyLogService)
        {
            Console.Clear();
            AnsiConsole.Clear();
            Thread.Sleep(100);
            AnsiConsole.MarkupLine($"[bold]{Localization.Get("delete_backup")}[/]");

            var backups = viewModel.GetBackupList();
            if (backups.Count == 0)
            {
                AnsiConsole.MarkupLine($"[red]{Localization.Get("no_backups")}[/]");
                WaitForBackspace();
                return;
            }

            var selectedBackups = AnsiConsole.Prompt(
                new MultiSelectionPrompt<string>()
                    .Title(Localization.Get("Select_del"))
                    .PageSize(10)
                    .InstructionsText($"[grey]{Localization.Get("Use_space")}[/]")
                    .AddChoices(backups.ConvertAll(b => b.Name)));

            if (selectedBackups.Count == 0)
            {
                AnsiConsole.MarkupLine($"[yellow]{Localization.Get("no_backup_select")}[/]");
                return;
            }

            List<string> allStatuses = new List<string>();

            Parallel.ForEach(selectedBackups, (backupName) =>
            {
                var selectedBackup = viewModel.GetBackupByName(backupName);
                if (selectedBackup != null)
                {
                    var stopwatch = new Stopwatch();
                    stopwatch.Start();
                    viewModel.DeleteBackup(new List<string> { selectedBackup.Name });
                    stopwatch.Stop();

                    long transferTime = stopwatch.ElapsedMilliseconds;

                    dailyLogService.WriteLogEntry(new LogEntry
                    {
                        Timestamp = DateTime.Now,
                        BackupName = selectedBackup.Name,
                        SourcePath = selectedBackup.SourcePath,
                        DestinationPath = selectedBackup.DestinationPath,
                        FileSize = dailyLogService.GetSize(selectedBackup.SourcePath, selectedBackup.DestinationPath, selectedBackup.Name),
                        Time = transferTime + "ms",
                        Type = "Delete"
                    });

                    lock (allStatuses)
                    {
                        allStatuses.Add($"{selectedBackup.Name} {Localization.Get("backup_completed_dele")} {transferTime} ms.");
                    }
                }
            });

            AnsiConsole.MarkupLine($"[bold blue]{string.Join("\n", allStatuses)}[/]");
            WaitForBackspace();
        }

        static void WaitForBackspace()
        {
            AnsiConsole.Markup($"[bold yellow]{Localization.Get("press_backspace")}[/]");
            while (Console.ReadKey(true).Key != ConsoleKey.Backspace) { }
        }
    }
}