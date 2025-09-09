using BepInEx.Configuration;

namespace HollywoodGraphics;

public static class ConfigurationTemplates
{
    public static void SetJanky(ConfigFile mainConfig)
    {
        // First reset to the defaults
        SetDefaults(mainConfig);
        
        Plugin.GraphicsConfig.SetMapConfig("Customs", true, 4f, 2.5f, 2f);
        Plugin.GraphicsConfig.SetMapConfig("FactoryDay", true, 10f, 2.5f, 2f);
        Plugin.GraphicsConfig.SetMapConfig("FactoryNight", true, 10f, 2.5f, 2f);
        Plugin.GraphicsConfig.SetMapConfig("Interchange", true, 4f, 2.5f, 2f);
        Plugin.GraphicsConfig.SetMapConfig("Lighthouse", true, 8f, 2.5f, 2f);
        Plugin.GraphicsConfig.SetMapConfig("Reserve", true, 8f, 2.5f, 2f);
        Plugin.GraphicsConfig.SetMapConfig("GroundZero", true, 4f, 2.5f, 2f);
        Plugin.GraphicsConfig.SetMapConfig("Shoreline", true, 8f, 2.5f, 2f);
        Plugin.GraphicsConfig.SetMapConfig("Woods", true, 8f, 2.5f, 2f);

        Plugin.GraphicsConfig.ToggleTonemaps(true);
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
        
        Plugin.GraphicsConfig.SetMapConfig("Customs", true, 1f, 0.5f, 0.5f);
        Plugin.GraphicsConfig.SetMapConfig("FactoryDay", true, 1f, 0.5f, 0.5f);
        Plugin.GraphicsConfig.SetMapConfig("FactoryNight", true, 1f, 0.5f, 0.5f);
        Plugin.GraphicsConfig.SetMapConfig("Interchange", true, 1f, 0.5f, 0.5f);
        Plugin.GraphicsConfig.SetMapConfig("Lighthouse", true, 1f, 0.5f, 0.5f);
        Plugin.GraphicsConfig.SetMapConfig("Reserve", true, 1f, 0.5f, 0.5f);
        Plugin.GraphicsConfig.SetMapConfig("GroundZero", true, 1f, 0.5f, 0.5f);
        Plugin.GraphicsConfig.SetMapConfig("Shoreline", true, 1f, 0.5f, 0.5f);
        Plugin.GraphicsConfig.SetMapConfig("Streets", true, 1f, 0.25f, 0.25f);
        Plugin.GraphicsConfig.SetMapConfig("Woods", true, 1f, 0.5f, 0.5f);
    }

    public static void SetDisabled(ConfigFile mainConfig)
    {
        // First reset to the defaults
        SetDefaults(mainConfig);
        
        Plugin.GraphicsConfig.Bloom.UseLensDust.Value = false;
        Plugin.GraphicsConfig.Bloom.UseAnamorphicFlare.Value = false;
        Plugin.GraphicsConfig.Bloom.UseStarFlare.Value = false;

        Plugin.GraphicsConfig.AOEnabled.Value = false;
        Plugin.GraphicsConfig.AOMultiBounceEnabled.Value = false;
        Plugin.GraphicsConfig.AOColorBleedEnabled.Value = false;
        
        Plugin.GraphicsConfig.LightFlareEnabled.Value = false;

        Plugin.GraphicsConfig.SunColorEnabled.Value = false;
        
        Plugin.GraphicsConfig.ToggleTonemaps(false);
    }
    
    public static void SetDefaults(ConfigFile mainConfig)
    {
        foreach (var pair in mainConfig)
        {
            pair.Value.BoxedValue = pair.Value.DefaultValue;
        }
    }
}