using System.IO.Compression;
using System.Runtime.InteropServices;
using ServerGate.Features.Main;
using Spectre.Console;

namespace ServerGate.Features.Server;

public static class Utils
{
    public static bool TryGetExecutable(out string executable)
    {
        executable = "SCPPR.x86_64";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            executable = "SCPPR.exe";

        if (File.Exists(executable))
            return true;

        ConsoleLogger.Alert("\nExecutable not found, make sure this file is on the same folder as ServerGate.");
        ConsoleLogger.ReadKey();
        return false;
    }

    private static void ArchiveDirectory(DirectoryInfo directory)
    {
        using FileStream zipFile = File.Open(directory.FullName + ".zip", FileMode.Create);
        using ZipArchive archive = new(zipFile, ZipArchiveMode.Create);

        foreach (FileInfo fileInfo in directory.EnumerateFiles("*.log"))
        {
            archive.CreateEntryFromFile(fileInfo.FullName, fileInfo.Name);
            fileInfo.Delete();
        }
    }

    private static void ArchiveDay(string path, string day)
    {
        string pathDay = Path.Combine(path, day);

        if (!Directory.Exists(pathDay))
            return;

        ArchiveDirectory(new DirectoryInfo(pathDay));
    }

    private static void DeleteDay(string path, string day)
    {
        string pathDay = Path.Combine(path, day);

        if (File.Exists(pathDay + ".zip"))
            File.Delete(pathDay + ".zip");

        if (!Directory.Exists(pathDay))
            return;

        Directory.Delete(pathDay, true);
    }

    public static void ArchiveOldLogs(int days)
    {
        DateTime today = ConsoleLogger.GetDateTimeWithOffset();
        for (int i = 1; i < days; i++)
        {
            string day = today.AddDays(-i).ToString("yyyy-MM-dd");

            ArchiveDay(FileManager.ProgramLogsFolder, day);
            ArchiveDay(FileManager.ServerLogsFolder, day);
        }
    }

    public static void RemoveOldLogs(int days)
    {
        DateTime today = ConsoleLogger.GetDateTimeWithOffset();
        for (int i = 1; i < days; i++)
        {
            string day = today.AddDays(-i).ToString("yyyy-MM-dd");

            DeleteDay(FileManager.ProgramLogsFolder, day);
            DeleteDay(FileManager.ServerLogsFolder, day);
        }
    }

    public static void SaveCrashLogs()
    {
        DateTime now = ConsoleLogger.GetDateTimeWithOffset();
        string day = now.ToString("yyyy-MM-dd");

        Directory.CreateDirectory(Path.Combine(FileManager.ProgramLogsFolder, day));
        File.WriteAllText(Path.Combine(FileManager.ProgramLogsFolder, day, $"{now:hh-mm-ss}-crash.log"), AnsiConsole.ExportText());
    }

    public static void SaveLogs()
    {
        DateTime now = ConsoleLogger.GetDateTimeWithOffset();
        string day = now.ToString("yyyy-MM-dd");

        Directory.CreateDirectory(Path.Combine(FileManager.ProgramLogsFolder, day));
        File.WriteAllText(Path.Combine(FileManager.ProgramLogsFolder, day, $"{now:hh-mm-ss}.log"), AnsiConsole.ExportText());
    }
}