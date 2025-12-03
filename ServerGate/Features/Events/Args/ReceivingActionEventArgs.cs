using ServerGate.Features.Server;

namespace ServerGate.Features.Events.Args;

public class ReceivingActionEventArgs(OutputCodes code) : EventArgs
{
    public bool IsAllowed { get; set; } = true;
    public OutputCodes ActionCode { get; set; } = code;
}