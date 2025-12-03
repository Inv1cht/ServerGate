namespace ServerGate.Features.Server.Commands;

[AttributeUsage(AttributeTargets.Method)]
public class CommandAttribute(string name, string[] aliases = null) : Attribute
{
    public string Name { get; } = name;
    public string[] Aliases { get; } = aliases ?? [];
}