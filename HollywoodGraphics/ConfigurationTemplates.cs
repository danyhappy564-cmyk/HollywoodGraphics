using BepInEx.Configuration;

namespace HollywoodGraphics;

public static class ConfigurationTemplates
{
    public static void SetJanky(ConfigFile mainConfig)
    {
        // First reset to the defaults
        SetDefaults(mainConfig);
        
        Plugin.GraphicsConfig.SetMapConfig("Customs", true, 4f, 2.5f, 2f);
        Plugin.GraphicsConfig.SetMapConfig("Interchange", true, 4f, 2.5f, 2f);
        Plugin.GraphicsConfig.SetMapConfig("Lighthouse", true, 8f, 2.5f, 2f);
        Plugin.GraphicsConfig.SetMapConfig("Reserve", true, 4f, 2.5f, 2f);
        Plugin.GraphicsConfig.SetMapConfig("GroundZero", true, 4f, 2.5f, 2f);
        Plugin.GraphicsConfig.SetMapConfig("Shoreline", true, 8f, 2.5f, 2f);
        Plugin.GraphicsConfig.SetMapConfig("Woods", true, 8f, 2.5f, 2f);
    }

    public static void SetPotato(ConfigFile mainConfig)
    {
        // First reset to the defaults
        SetDefaults(mainConfig);
    }
    
    public static void SetDefaults(ConfigFile mainConfig)
    {
        foreach (var pair in mainConfig)
        {
            pair.Value.BoxedValue = pair.Value.DefaultValue;
        }
    }
}