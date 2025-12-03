using ServerGate.Features.Server;
using ServerGate.Features.Events;
using ServerGate.Features.Events.Args;

namespace ServerGate.Features.Main;

public static class InputManager
{
    private static bool _isReading = true;

    public static bool Stop() => _isReading = false;

    public static void Start()
    {
        while (_isReading)
        {
            string input = System.Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input))
                continue;

            input = input.TrimStart();

            ReceivingInputEventArgs args = new(input);
            Handler.OnReceivingInput(args);

            if (!args.IsAllowed)
                continue;

            input = args.Input;

            if (!Program.CommandHandler.SendCommand(input))
                ManageInput(input);
        }
    }

    private static void ManageInput(string input)
    {
        if (Program.Server.Status == ServerStatus.Offline)
        {
            ConsoleLogger.Alert("The server hasn't been initialized yet.");
            return;
        }

        ConsoleLogger.Input(input);

        Program.Server?.TcpServer?.SendMessage(input);
    }
}