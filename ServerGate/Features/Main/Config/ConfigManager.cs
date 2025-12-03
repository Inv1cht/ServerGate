using YamlDotNet.Serialization;

namespace ServerGate.Features.Main.Config;

public class ConfigManager
{
    public MainConfig Config = new();
    public static readonly Serializer Serializer = new();
    public static readonly Deserializer Deserializer = new();

    public void LoadConfig()
    {
        if (!File.Exists(FileManager.ProgramConfig))
        {
            while (true)
            {
                ProgramIntroduction.ShowIntroduction();

                if (ConsoleLogger.GetConfirm("Do you want to configure again ServerGate?", false))
                    continue;

                break;
            }
        }

        Config = Deserializer.Deserialize<MainConfig>(File.ReadAllText(FileManager.ProgramConfig));
        File.WriteAllText(FileManager.ProgramConfig, Serializer.Serialize(Config));
    }

    public void SaveConfig(MainConfig config)
    {
        File.WriteAllText(FileManager.ProgramConfig, Serializer.Serialize(config));
        LoadConfig();
    }
}