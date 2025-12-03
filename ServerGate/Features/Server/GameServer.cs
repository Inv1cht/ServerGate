using System.Diagnostics;
using ServerGate.Features.Console;
using ServerGate.Features.Main;
using Spectre.Console;
using ServerGate.Features.Events;
using ServerGate.Features.Events.Args;

namespace ServerGate.Features.Server;

public class GameServer(int port = 7777, string[] args = null)
{
    private Process _process;

    private readonly int _port = port;
    private readonly string[] _args = args;

    public ServerStatus Status = ServerStatus.Offline;
    public TcpServer TcpServer;

    public SilentCrashHandler SilentCrashHandler;

    private bool _logStdErr;
    private bool _logStdOut;

    private Logger _serverLogger;
    private Logger _outputLogger;

    public void Start()
    {
        StartingServerEventArgs args = new(this);
        Handler.OnStartingServer(args);

        if (!args.IsAllowed)
            return;

        TcpServer = new TcpServer();

        StartServerProcess();
    }

    private void StartServerProcess()
    {
        _process?.Dispose();

        if (!Utils.TryGetExecutable(out string executablePath))
        {
            Environment.Exit(-1);
            return;
        }

        System.Console.Title = $"SCPPR | ServerGate [v{Program.Version}] - {_port}";

        DateTime now = ConsoleLogger.GetDateTimeWithOffset();
        string day = now.ToString("yyyy-MM-dd");

        Directory.CreateDirectory(Path.Combine(FileManager.ServerLogsFolder, day));

        _serverLogger = new Logger(Path.Combine(FileManager.ServerLogsFolder, day, $"{now:hh-mm-ss}-server.log"));
        _outputLogger = new Logger(Path.Combine(FileManager.ServerLogsFolder, day, $"{now:hh-mm-ss}-output.log"));

        List<string> gameArgs =
        [
            "-batchmode",
            "-nographics",
            "-silent-crashes",
            "-nodedicateddelete",
            $"-id {Environment.ProcessId}",
            $"-tcp {TcpServer.Port}",
            $"-port {_port}",
        ];

        if (Program.ConfigManager.Config.RestartOnCrash)
            gameArgs.Add("-heartbeat");

        ProcessStartInfo startInfo = new(executablePath, string.Join(' ', gameArgs) + ' ' + string.Join(' ', _args ?? []))
        {
            CreateNoWindow = true,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            RedirectStandardInput = true,
        };

        _process = Process.Start(startInfo);

        if (_process is null)
        {
            ConsoleLogger.Alert("An error occurred when starting the server process.");
            ConsoleLogger.ReadKey();
            Environment.Exit(-1);
        }

        _process.EnableRaisingEvents = true;

        _process.Exited += OnExited;
        _process.OutputDataReceived += StdOut;
        _process.ErrorDataReceived += StdErr;
        _process.BeginOutputReadLine();
        _process.BeginErrorReadLine();

        Status = ServerStatus.Online;

        if (Program.ConfigManager.Config.RestartOnCrash)
            SilentCrashHandler = new SilentCrashHandler();
    }

    private void KillSilentCrashHandler()
    {
        if (SilentCrashHandler is null)
            return;

        SilentCrashHandler.Stop();
        SilentCrashHandler = null;
    }

    private void KillSocket()
    {
        if (TcpServer is null)
            return;

        TcpServer.Stop();
        TcpServer = null;
    }

    public void Stop()
    {
        if (_process == null || _process.HasExited)
        {
            ConsoleLogger.Alert("Server process is not running.");
            return;
        }

        StoppingServerEventArgs args = new(this);
        Handler.OnStoppingServer(args);

        if (!args.IsAllowed) return;

        try
        {
            using (StreamWriter sw = _process.StandardInput)
            {
                if (sw.BaseStream.CanWrite)
                {
                    sw.WriteLine("shutdown");
                    sw.Flush();
                }
            }
        }
        catch (Exception ex)
        {
            ConsoleLogger.Alert($"Failed to send shutdown command: {ex.Message}");
        }

        if (!_process.WaitForExit(1000))
            _process.Kill();


        Status = ServerStatus.Offline;

        KillSilentCrashHandler();
        KillSocket();

        ConsoleLogger.SpectreRaw("Goodbye!", "green");
    }

    public void Restart()
    {
        RestartingServerEventArgs args = new(this);
        Handler.OnRestartingServer(args);

        if (!args.IsAllowed)
            return;

        System.Console.Clear();

        Status = ServerStatus.Offline;

        KillSilentCrashHandler();
        TcpServer.Reset();

        _process.Kill();

        StartServerProcess();
    }

    private void OnExited(object o, EventArgs e)
    {
        switch (Status)
        {
            case ServerStatus.RestartingNextRound:
                {
                    Restart();
                    break;
                }
            case ServerStatus.Offline:
                {
                    break;
                }
            case ServerStatus.Exiting:
            case ServerStatus.ExitingNextRound:
                {
                    Stop();
                    Environment.Exit(0);
                    break;
                }
            case ServerStatus.Idle:
            case ServerStatus.Online:
                {
                    ConsoleLogger.Raw(@"" +
                            @"   █████████                              █████      ███" +
                            "  ███░░░░░███                            ░░███      ░███" +
                            " ███     ░░░  ████████   ██████    █████  ░███████  ░███" +
                            "░███         ░░███░░███ ░░░░░███  ███░░   ░███░░███ ░███" +
                            "░███          ░███ ░░░   ███████ ░░█████  ░███ ░███ ░███" +
                            "░░███     ███ ░███      ███░░███  ░░░░███ ░███ ░███ ░░░ " +
                            " ░░█████████  █████    ░░████████ ██████  ████ █████ ███" +
                            "  ░░░░░░░░░  ░░░░░      ░░░░░░░░ ░░░░░░  ░░░░ ░░░░░ ░░░ ", ConsoleColor.DarkYellow, false);

                    if (Program.ConfigManager.Config.RestartOnCrash)
                        Restart();
                    else
                        ConsoleLogger.ReadKey();
                    break;
                }
        }
    }

    public void ToggleStdOut() => _logStdOut = !_logStdOut;

    public void ToggleStdErr() => _logStdErr = !_logStdErr;


    private void StdOut(object _, DataReceivedEventArgs ev)
    {
        if (_logStdOut)
            ConsoleLogger.SpectreRaw("[[STDOUT]]" + ev.Data.EscapeMarkup(), "paleturquoise4");

        AddOutputLog(ev.Data, "STDOUT");
    }

    private void StdErr(object _, DataReceivedEventArgs ev)
    {
        if (_logStdErr)
            ConsoleLogger.SpectreRaw("[[STDERR]]" + ev.Data.EscapeMarkup(), "indianred");

        AddOutputLog(ev.Data, "STDERR");
    }

    public void AddLog(string message, string title = "")
    {
        if (string.IsNullOrEmpty(message))
            return;

        _serverLogger.AppendLog(string.IsNullOrWhiteSpace(title) ? message : $"[{title}] {message}");
    }

    private void AddOutputLog(string message, string title = "")
    {
        if (string.IsNullOrEmpty(message))
            return;

        _outputLogger.AppendLog(string.IsNullOrWhiteSpace(title) ? message : $"[{title}] {message}");
    }
}

public enum ServerStatus
{
    Online,
    Offline,
    Idle,
    Exiting,
    ExitingNextRound,
    RestartingNextRound,
}