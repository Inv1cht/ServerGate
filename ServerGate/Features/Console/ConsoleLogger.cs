using ServerGate.Features.Console;
using Spectre.Console;

namespace ServerGate.Features;

public static class ConsoleLogger
{
    // Program Alerts
    public static void Intro()
    {
        System.Console.Clear();
        WriteLine(@"[red]   _____                           ______      __     
  / ___/___  ______   _____  _____/ ____/___ _/ /____ 
  \__ \/ _ \/ ___/ | / / _ \/ ___/ / __/ __ `/ __/ _ \
 ___/ /  __/ /   | |/ /  __/ /  / /_/ / /_/ / /_/  __/
/____/\___/_/    |___/\___/_/   \____/\__,_/\__/\___/ 
                                                      [/]");
        Write($"[chartreuse3]ServerGate v{Program.Version}[/]");
        WriteLine(" by [cyan]Inv1cht[/]");
        WriteLine($"[thistle1]Released under [blue]MIT License[/] Copyright © [cyan]Inv1cht[/] 2024-{DateTime.Now.Year}[/]");
    }

    public static DateTime GetDateTimeWithOffset() => DateTime.Now.AddHours(Program.ConfigManager.Config.TimeOffset);
    public static string GetTimeWithOffset() => GetDateTimeWithOffset().ToString("T");

    public static void Input(string message, string title = "SERVER")
    {
        Write($"[mistyrose1]{title} >>> [/]");
        WriteLine(message.EscapeMarkup());
    }

    public static void Alert(string message, bool showTimeStamp = true)
    {
        if (showTimeStamp)
            AddTimeStamp();

        Write("ServerGate ", ConsoleColor.Yellow);
        System.Console.Write("(Alert) ");
        WriteLine(message, ConsoleColor.Gray);
    }

    public static void ReadKey()
    {
        System.Console.WriteLine();
        AnsiConsole.Write(new Rule("[darkslategray3]Press any key to continue.[/]"));
        System.Console.ReadKey();
    }

    // Alerts

    public static void Path(string path)
    {
        TextPath p = new(path)
        {
            RootStyle = new Style(foreground: Color.Red),
            SeparatorStyle = new Style(foreground: Color.Green),
            StemStyle = new Style(foreground: Color.Blue),
            LeafStyle = new Style(foreground: Color.Yellow)
        };

        AnsiConsole.Write(p);
        System.Console.WriteLine();
    }

    public static void Raw(string message, ConsoleColor color = ConsoleColor.White, bool showTimeStamp = true)
    {
        if (showTimeStamp)
            AddTimeStamp();

        WriteLine(message.EscapeMarkup(), color);
    }

    public static void SpectreRaw(string message, string color = "white", bool showTimeStamp = false)
    {
        if (showTimeStamp)
            AddTimeStamp();

        WriteLine($"[{color}]{message}[/]");
    }

    private static void AddTimeStamp() => AnsiConsole.Markup($"[paleturquoise4][[[deepskyblue4_1]{GetTimeWithOffset()}[/]]][/] ");

    public static void WriteLine(string message, ConsoleColor color = ConsoleColor.White)
    {
        System.Console.ForegroundColor = color;
        AnsiConsole.MarkupLine(message);
        System.Console.ForegroundColor = ConsoleColor.White;
    }

    public static void Write(string message, ConsoleColor color = ConsoleColor.White)
    {
        System.Console.ForegroundColor = color;
        AnsiConsole.Markup(message);
        System.Console.ForegroundColor = ConsoleColor.White;
    }

    public static int GetOption(string msg, int def)
    {
        AnsiConsole.MarkupLine($"[lightcyan3]{msg}[/] [springgreen3]({def})[/]:");

        while (true)
        {
            string input = System.Console.ReadLine();

            if (string.IsNullOrEmpty(input))
                return def;

            if (input is "exit")
                Environment.Exit(-1);

            if (int.TryParse(input, out int result))
                return result;

            AnsiConsole.MarkupLine($"[red]Invalid Input[/] Try again with a number:");
        }
    }

    public static bool GetConfirm(string msg, bool def)
    {
        AnsiConsole.MarkupLine($"[lightcyan3]{msg}[/] [dodgerblue1](y/n)[/] [springgreen3]({(def ? "Yes" : "No")})[/]:");

        while (true)
        {
            string input = System.Console.ReadLine();

            if (string.IsNullOrEmpty(input))
                return def;

            if (input is "exit")
                Environment.Exit(-1);

            switch (input.Trim().ToLower())
            {
                case "yes" or "y":
                    return true;
                case "no" or "n":
                    return false;
            }

            AnsiConsole.MarkupLine($"[red]Invalid Input[/] Try again, it only accepts y or yes and n or no.");
        }
    }

    public static void HandleMessage(string message, Colors code)
    {
        if (string.IsNullOrWhiteSpace(message)) return;

        if (!Enum.IsDefined(typeof(Colors), code))
        {
            Raw(message, Colors.Gray.ToConsoleColor());
            return;
        }


        string color = code switch
        {
            Colors.Black => "black",
            Colors.White => "white",
            Colors.Red => "red",
            Colors.Green => "green",
            Colors.Blue => "blue",
            Colors.Yellow => "yellow",
            Colors.Cyan => "cyan1",
            Colors.Magenta => "magenta1",
            Colors.Grey => "grey", 
            Colors.Gray => "grey54",
            _ => "grey54"
        };

        SpectreRaw(message.EscapeMarkup(), color, true);
    }
}