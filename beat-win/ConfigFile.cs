using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace beat_win;

public class ConfigFile
{
    public bool RaylibRenderer { get; set; } = false;

    public static ConfigFile Instance = new ConfigFile();
    public static void Load()
    {
        try
        {
            Instance =
                JsonSerializer.Deserialize<ConfigFile>(File.ReadAllText("config.json"))
                ?? Instance;
        }
        catch { }
    }
    public static void Save()
    {
        File.WriteAllText("config.json", JsonSerializer.Serialize<ConfigFile>(Instance));
    }
}
