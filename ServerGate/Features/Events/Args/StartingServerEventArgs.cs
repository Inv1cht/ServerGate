using ServerGate.Features.Server;

namespace ServerGate.Features.Events.Args;

public class StartingServerEventArgs(GameServer server) : EventArgs
{
    public bool IsAllowed { get; set; } = true;
    public GameServer Server { get; } = server;
}