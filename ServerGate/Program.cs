using System.Reflection;
using ServerGate.Features;
using ServerGate.Features.Main.Config;
using Spectre.Console;
using ServerGate.Features.Main;
using ServerGate.Features.Server;
using ServerGate.Features.Server.Commands;

namespace ServerGate;

public static class Program
{
    public static Version? Version { get; private set; }
    public static GameServer Server { get; private set; }
    public static CommandHandler CommandHandler { get; private set; }
    public static ConfigManager ConfigManager { get; private set; }

    public static int Port { get; private set; }

    public static bool _exceptionalExit;

    static void Main(string[] args)
    {
        Version = Assembly.GetExecutingAssembly().GetName().Version;

        Console.Title = $"SCPPR | ServerGate [v{Version}]";

        AppDomain.CurrentDomain.ProcessExit += OnExit;
        AppDomain.CurrentDomain.UnhandledException += OnError;
        Console.CancelKeyPress += new ConsoleCancelEventHandler(Exit);

        AnsiConsole.Record();

        Start(args);
    }

    private static void Start(string[] args)
    {
        if (args.Length == 0 || !int.TryParse(args[0], out int port))
            port = ConsoleLogger.GetOption("What port do you want to start the server on?", 7777);
        else
            args = args.Skip(1).ToArray();

        Port = port;

        ConsoleLogger.Intro();

        FileManager.Load();

        ConfigManager = new ConfigManager();
        ConfigManager.LoadConfig();

        Utils.RemoveOldLogs(ConfigManager.Config.DeleteLogsDays);
        Utils.ArchiveOldLogs(ConfigManager.Config.ArchiveLogsDays);

        CommandHandler = new CommandHandler();

        Server = new GameServer(port, args);
        Server.Start();

        InputManager.Start();
    }

    private static void OnError(object obj, UnhandledExceptionEventArgs ev)
    {
        _exceptionalExit = true;

        if (FileManager.MainFolder is null)
            FileManager.Load();

        try
        {
            AnsiConsole.WriteException((Exception)ev.ExceptionObject);
            Utils.SaveCrashLogs();
        }
        catch (Exception e)
        {
            AnsiConsole.WriteException(e);
        }
    }

    private static void OnExit(object obj, EventArgs ev)
    {
        if (FileManager.MainFolder is null)
            FileManager.Load();

        if (!_exceptionalExit)
        {
            try { Utils.SaveLogs(); }
            catch { Utils.SaveCrashLogs(); }
        }

        Shutdown();
    }

    private static void Exit(object obj, ConsoleCancelEventArgs ev)
    {
        ev.Cancel = true;
        _exceptionalExit = true;
        Environment.Exit(0);
    }

    private static void Shutdown()
    {
        Server?.Stop();
        InputManager.Stop();
    }
}