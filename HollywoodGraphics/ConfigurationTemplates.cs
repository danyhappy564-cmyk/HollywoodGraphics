using BepInEx.Configuration;

namespace HollywoodGraphics;

public static class ConfigurationTemplates
{
    public static void SetJanky(ConfigFile mainConfig)
    {
        // First reset to the defaults
        SetDefaults(mainConfig);

        Plugin.GraphicsConfig.SetMapLodConfig("Customs", true, 2.5f, 2f);
        Plugin.GraphicsConfig.SetMapLodConfig("Interchange", true,  2.5f, 2f);
        Plugin.GraphicsConfig.SetMapLodConfig("Lighthouse", true,  2.5f, 2f);
        Plugin.GraphicsConfig.SetMapLodConfig("Reserve", true,  2.5f, 2f);
        Plugin.GraphicsConfig.SetMapLodConfig("GroundZero", true,  2.5f, 2f);
        Plugin.GraphicsConfig.SetMapLodConfig("Shoreline", true,  2.5f, 2f);
        Plugin.GraphicsConfig.SetMapLodConfig("Woods", true,  2.5f, 2f);
    }

    public static void SetPotato(ConfigFile mainConfig)
    {
        // First reset to the defaults
        SetDefaults(mainConfig);

        Plugin.GraphicsConfig.Bloom.UseLensDust.Value = false;
        Plugin.GraphicsConfig.Bloom.UseAnamorphicFlare.Value = false;
        Plugin.GraphicsConfig.Bloom.UseStarFlare.Value = false;

        Plugin.GraphicsConfig.AOIntensity.Value = 0.75f;
        Plugin.GraphicsConfig.AOMultiBounceEnabled.Value = false;
        Plugin.GraphicsConfig.AOColorBleedEnabled.Value = false;
        
        Plugin.GraphicsConfig.SetMapLodConfig("Customs", false);
        Plugin.GraphicsConfig.SetMapLodConfig("Interchange", false);
        Plugin.GraphicsConfig.SetMapLodConfig("Lighthouse", false);
        Plugin.GraphicsConfig.SetMapLodConfig("Reserve", false);
        Plugin.GraphicsConfig.SetMapLodConfig("GroundZero", false);
        Plugin.GraphicsConfig.SetMapLodConfig("Shoreline", false);
        Plugin.GraphicsConfig.SetMapLodConfig("Woods", false);
    }
    
    public static void SetDefaults(ConfigFile mainConfig)
    {
        foreach (var pair in mainConfig)
        {
            pair.Value.BoxedValue = pair.Value.DefaultValue;
        }
    }
}