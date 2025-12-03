using System.Net;
using System.Net.Sockets;
using System.Text;
using Spectre.Console;
using ServerGate.Features.Events;
using ServerGate.Features.Events.Args;
using ServerGate.Features.Main;

namespace ServerGate.Features.Server;

public class TcpServer
{
    public int Port;

    private TcpListener _listener;
    private TcpClient _client;
    private NetworkStream _stream;

    private CancellationTokenSource _cancellationTokenSource = new();

    public TcpServer() => Start();

    private void Start()
    {
        _listener = new TcpListener(new IPEndPoint(IPAddress.Loopback, 0));
        _listener.Start();

        Port = ((IPEndPoint)_listener.LocalEndpoint).Port;
        _cancellationTokenSource = new CancellationTokenSource();

        _listener.BeginAcceptTcpClient(asyncResult =>
        {
            _client = _listener.EndAcceptTcpClient(asyncResult);
            _stream = _client.GetStream();

            Task.Run(ListenRequests);
        }, _listener);
    }

    public void Reset()
    {
        Stop();
        Start();
    }

    public void Stop()
    {
        _cancellationTokenSource.Cancel();
        _listener.Stop();
        _client.Close();
    }

    private async void ListenRequests()
    {
        byte[] codeBuffer = new byte[1]; // use size of byte since the code is a single byte
        byte[] lengthBuffer = new byte[sizeof(int)]; // use size of an int since the length is sent as an int (4 bytes)

        try
        {
            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                // First byte is the output code
                int codeBytes = await _stream.ReadAsync(codeBuffer.AsMemory(0, 1), _cancellationTokenSource.Token);
                if (codeBytes <= 0) break;

                byte codeType = codeBuffer[0];

                // Skip non-coloured messages
                if (codeType > 0xF)
                {
                    HandleAction(codeType);
                    continue;
                }

                // 4 bytes for the length
                int lengthBytes = await _stream.ReadAsync(lengthBuffer.AsMemory(0, sizeof(int)), _cancellationTokenSource.Token);
                if (lengthBytes != sizeof(int)) break;

                // Convert the length from the byte array
                int length = BitConverter.ToInt32(lengthBuffer, 0);
                if (length <= 0) break; // sanity check for length

                // Read the message of the specified length
                byte[] messageBuffer = new byte[length];
                int messageBytesRead = await _stream.ReadAsync(messageBuffer.AsMemory(0, length), _cancellationTokenSource.Token);
                if (messageBytesRead <= 0) break; // handle disconnection

                // Convert the message bytes to a string
                string message = Encoding.UTF8.GetString(messageBuffer, 0, messageBytesRead);

                ReceivingMessageEventArgs args = new(message, codeType);
                Handler.OnReceivingMessage(args);

                if (!args.IsAllowed) continue;

                Program.Server.AddLog(args.Message, ConsoleLogger.GetTimeWithOffset());
                ConsoleLogger.HandleMessage(args.Message, args.Color);
            }
        }
        catch (Exception ex)
        {
            ConsoleLogger.Alert($"Socket error: {ex.Message}");
        }
        finally
        {
            _cancellationTokenSource.Cancel();
        }
    }

    public void SendMessage(string message)
    {
        if (_stream == null)
        {
            ConsoleLogger.Alert("The server hasn't been initialized yet");
            return;
        }

        byte[] messageBuffer = new byte[Encoding.UTF8.GetMaxByteCount(message.Length) + sizeof(int)];
        int actualMessageLength = Encoding.UTF8.GetBytes(message, 0, message.Length, messageBuffer, sizeof(int));

        Array.Copy(BitConverter.GetBytes(actualMessageLength), messageBuffer, sizeof(int));

        try
        {
            if (_stream.CanWrite)
                _stream.Write(messageBuffer, 0, actualMessageLength + sizeof(int));
        }
        catch (Exception e)
        {
            if (e is IOException)
                return;

            AnsiConsole.WriteException(e);
        }
    }

    private static void HandleAction(byte action)
    {
        ReceivingActionEventArgs args = new((OutputCodes)action);
        Handler.OnReceivingAction(args);

        if (!args.IsAllowed)
            return;

        switch (args.ActionCode)
        {
            case OutputCodes.RoundRestart:
                Program.Server.AddLog("Waiting for players.");
                break;

            case OutputCodes.IdleEnter:
                Program.Server.Status = ServerStatus.Idle;
                Program.Server.AddLog("Server entered idle mode.");
                break;

            case OutputCodes.IdleExit:
                Program.Server.Status = ServerStatus.Online;
                Program.Server.AddLog("Server exited idle mode.");
                break;

            case OutputCodes.ExitActionReset:
                Program.Server.Status = ServerStatus.Online;
                break;

            case OutputCodes.ExitActionShutdown:
                Program.Server.Status = ServerStatus.ExitingNextRound;
                break;

            case OutputCodes.ExitActionSilentShutdown:
                Program.Server.Status = ServerStatus.ExitingNextRound;
                break;

            case OutputCodes.ExitActionRestart:
                Program.Server.Status = ServerStatus.RestartingNextRound;
                break;

            case OutputCodes.Heartbeat:
                Program.Server.SilentCrashHandler?.OnReceivedHeartbeat();
                break;

            default:
                ConsoleLogger.Alert($"Received unknown output code ({action}), possible buffer spam.");
                break;
        }
    }
}

public enum OutputCodes : byte
{
    // ServerOutput.OutputCodes

    // 0x00 - 0x0F - Reserved for ConsoleColor enum.
    // 0x10 - 0x17 - OutputCodes

    RoundRestart = 0x10,
    IdleEnter = 0x11,
    IdleExit = 0x12,
    ExitActionReset = 0x13,
    ExitActionShutdown = 0x14,
    ExitActionSilentShutdown = 0x15,
    ExitActionRestart = 0x16,
    Heartbeat = 0x17
}