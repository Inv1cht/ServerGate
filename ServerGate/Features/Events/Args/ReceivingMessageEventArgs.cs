using ServerGate.Features.Console;

namespace ServerGate.Features.Events.Args;

public class ReceivingMessageEventArgs(string message, byte color) : EventArgs
{
    public bool IsAllowed { get; set; } = true;
    public string Message { get; set; } = message;
    public Colors Color { get; set; } = (Colors)color;
}