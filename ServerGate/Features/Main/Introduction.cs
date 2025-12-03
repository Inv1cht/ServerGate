using System;
using System.IO;
using ServerGate.Features.Main.Config;
using ServerGate.Features;
using Spectre.Console;

namespace ServerGate.Features.Main;

public static class ProgramIntroduction
{
    public static void ShowIntroduction()
    {
        ConsoleLogger.Alert("\nHi, welcome to ServerGate!");
        ConsoleLogger.Alert("We have to configure some things before starting!\n");

        AnsiConsole.WriteLine("[red]Configuration[/]");

        // Program Options

        MainConfig cfg = new()
        {
            SafeShutdown = ConsoleLogger.GetConfirm("Do you want to safe shutdown the game processes?", true),
            RestartOnCrash = ConsoleLogger.GetConfirm("Should the server automatically restart itself when it crashes?", true),
            TimeOffset = ConsoleLogger.GetOption($"{DateTime.Now:T} In hours, write how many hours should be added/removed (For example -2).", 0),
            ArchiveLogsDays = ConsoleLogger.GetOption("In how many days the logs should be archived?", 1),
            DeleteLogsDays = ConsoleLogger.GetOption("In how many days the logs should be deleted?", 2),
        };

        Program.ConfigManager.SaveConfig(cfg);

        ConsoleLogger.SpectreRaw("\nThat were all the program configs! You can edit them always in:", "skyblue2");
        ConsoleLogger.Path(FileManager.ProgramConfig);

        Directory.CreateDirectory(FileManager.MainFolder);
    }
}