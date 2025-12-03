using System.Reflection;
using ServerGate.Features.Main;

namespace ServerGate.Features.Server.Commands;

public class CommandHandler
{
    public readonly Dictionary<string, MethodInfo> Commands = [];

    [Command("Reconfigure")]
    private static void ReconfigureServerGate()
    {
        ProgramIntroduction.ShowIntroduction();
        System.Console.WriteLine("Restart to apply the changes!");
    }

    [Command("StdErr")]
    private static void StdErr()
    {
        ConsoleLogger.Alert("StdErr logs toggled.");
        Program.Server.ToggleStdErr();
    }

    [Command("StdOut")]
    private static void StdOut()
    {
        ConsoleLogger.Alert("StdOut logs toggled.");
        Program.Server.ToggleStdOut();
    }

    [Command("Exit", ["Quit"])]
    private static void ExitCommand()
    {
        Program._exceptionalExit = true;
        Environment.Exit(0);
    }

    [Command("Restart")]
    private static void SoftRestartCommand()
    {
        ConsoleLogger.SpectreRaw("Restarting the server...", "lightslateblue");
        Program.Server.Restart();
        System.Console.Clear();
    }

    public CommandHandler()
    {
        TypeInfo ti = typeof(CommandHandler).GetTypeInfo();

        foreach (MethodInfo method in ti.DeclaredMethods)
        {
            IEnumerable<Attribute> attributes = method.GetCustomAttributes();

            if (attributes.FirstOrDefault() is CommandAttribute query)
            {
                Commands.Add(query.Name.ToLower(), method);

                foreach (string alias in query.Aliases)
                {
                    Commands.Add(alias.ToLower(), method);
                }
            }
        }
    }

    public bool SendCommand(string name)
    {
        name = name.ToLower();

        if (!Commands.TryGetValue(name, out MethodInfo? value)) return false;

        ConsoleLogger.Input(name, "ServerGate");
        value.Invoke(null, []);
        return true;
    }
}