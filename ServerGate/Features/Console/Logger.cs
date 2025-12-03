namespace ServerGate.Features.Console;

public class Logger(string path)
{
    private readonly string _path = path;

    public void AppendLog(object message, bool newLine = true)
    {

        using StreamWriter stream = File.AppendText(_path);

        if (newLine)
            stream.WriteLine(message);
        else
            stream.Write(message);
    }
}