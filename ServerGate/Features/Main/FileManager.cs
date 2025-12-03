using ServerGate.Features.Main.Config;

namespace ServerGate.Features.Main;

public static class FileManager
{
    public static string? MainFolder;
    public static string? LogsFolder;
    public static string? ServerLogsFolder;
    public static string? ProgramLogsFolder;
    public static string? ProgramConfig;

    public static void Load()
    {
        MainFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SCPPR");
        LogsFolder = Path.Combine(MainFolder, "Logs");
        ServerLogsFolder = Path.Combine(LogsFolder, Program.Port.ToString());
        ProgramLogsFolder = Path.Combine(LogsFolder, "ServerGate");
        ProgramConfig = Path.Combine(MainFolder, "ServerGate.yml");

        CreateIfNotExists();
    }

    private static void CreateIfNotExists()
    {
        Directory.CreateDirectory(MainFolder);
        Directory.CreateDirectory(LogsFolder);
        Directory.CreateDirectory(ServerLogsFolder);
        Directory.CreateDirectory(ProgramLogsFolder);
    }
}