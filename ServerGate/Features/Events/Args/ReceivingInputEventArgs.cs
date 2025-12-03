namespace ServerGate.Features.Events.Args;

public class ReceivingInputEventArgs(string input) : EventArgs
{
    public bool IsAllowed { get; set; } = true;
    public string Input { get; set; } = input;
}