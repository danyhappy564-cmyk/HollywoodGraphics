using System;
using System.Collections.Generic;
using BepInEx.Configuration;
using Comfort.Common;
using UnityEngine;

namespace HollywoodGraphics;

public class MapConfig(
    string name,
    ConfigEntry<bool> lodEnabled,
    ConfigEntry<float> detailDistance,
    ConfigEntry<float> detailDensity,
    ConfigEntry<float> bloomMultiplier
)
{
    public readonly string Name = name;

    public readonly ConfigEntry<bool> LodEnabled = lodEnabled;
    public readonly ConfigEntry<float> DetailDistance = detailDistance;
    public readonly ConfigEntry<float> DetailDensity = detailDensity;

    public readonly ConfigEntry<float> BloomMultiplier = bloomMultiplier;
}

public sealed class BloomConfig
{
    public readonly ConfigEntry<float> BloomIntensity;

    public readonly ConfigEntry<float> BloomDark;
    public readonly ConfigEntry<float> BloomMid;
    public readonly ConfigEntry<float> BloomBright;
    public readonly ConfigEntry<float> BloomHighlight;

    public readonly ConfigEntry<bool> UseLensDust;
    public readonly ConfigEntry<float> DustIntensity;
    public readonly ConfigEntry<float> DirtLightIntensity;

    public readonly ConfigEntry<bool> UseAnamorphicFlare;
    public readonly ConfigEntry<float> AnamorphicFlareIntensity;
    public readonly ConfigEntry<float> AnamorphicScale;
    public readonly ConfigEntry<int> AnamorphicBlurPass;

    public readonly ConfigEntry<bool> UseStarFlare;
    public readonly ConfigEntry<float> StarFlareIntensity;
    public readonly ConfigEntry<float> StarScale;
    public readonly ConfigEntry<int> StarBlurPass;
    public readonly ConfigEntry<string> LensDust;

    public BloomConfig(ConfigFile config)
    {
        const string bloomSection = "03. Bloom v1";

        BloomIntensity = config.Bind(bloomSection, "Master Bloom Intensity", 0.1f, new ConfigDescription(
            "Controls the overall intensity of the bloom effect.",
            new AcceptableValueRange<float>(0f, 1f),
            new ConfigurationManagerAttributes { Order = 104 }
        ));
        BloomIntensity.SettingChanged += OnConfigChanged;

        BloomDark = config.Bind(bloomSection, "Bloom Curve Dark", -0.98f, new ConfigDescription(
            "Bloom intensity of the dark colors range.",
            new AcceptableValueRange<float>(-3f, 3f),
            new ConfigurationManagerAttributes { Order = 103, IsAdvanced = true }
        ));
        BloomDark.SettingChanged += OnConfigChanged;

        BloomMid = config.Bind(bloomSection, "Bloom Curve Mid", 0.6f, new ConfigDescription(
            "Bloom intensity of the mid colors range.",
            new AcceptableValueRange<float>(-3f, 3f),
            new ConfigurationManagerAttributes { Order = 102, IsAdvanced = true }
        ));
        BloomMid.SettingChanged += OnConfigChanged;

        BloomBright = config.Bind(bloomSection, "Bloom Curve Bright", 0.75f, new ConfigDescription(
            "Bloom intensity of the bright colors range.",
            new AcceptableValueRange<float>(-3f, 3f),
            new ConfigurationManagerAttributes { Order = 101, IsAdvanced = true }
        ));
        BloomBright.SettingChanged += OnConfigChanged;

        BloomHighlight = config.Bind(bloomSection, "Bloom Curve Highlight", 0.6f, new ConfigDescription(
            "Bloom intensity of the bright colors range.",
            new AcceptableValueRange<float>(-3f, 3f),
            new ConfigurationManagerAttributes { Order = 100, IsAdvanced = true }
        ));
        BloomHighlight.SettingChanged += OnConfigChanged;

        UseLensDust = config.Bind(bloomSection, "Use Lens Dust", true, new ConfigDescription(
            "Enables lens dust effect.",
            null,
            new ConfigurationManagerAttributes { Order = 96 }
        ));
        UseLensDust.SettingChanged += (sender, args) =>
        {
            // Adjust the bloom intensity to compensate for this feature subduing overall bloom
            if (UseLensDust.Value)
            {
                BloomIntensity.Value *= 5f;
            }
            else
            {
                BloomIntensity.Value /= 5f;
            }

            OnConfigChanged(sender, args);
        };

        DustIntensity = config.Bind(bloomSection, "Lens Dust Amount", 0.3f, new ConfigDescription(
            "Controls the intensity of the lens dust effect.",
            new AcceptableValueRange<float>(0f, 5f),
            new ConfigurationManagerAttributes { Order = 95 }
        ));
        DustIntensity.SettingChanged += OnConfigChanged;

        LensDust = config.Bind(bloomSection, "Lens Dust Texture", "LensDust4A.png", new ConfigDescription(
            "Texture to use for the lens dust effect.",
            null,
            new ConfigurationManagerAttributes { Order = 94 }
        ));
        LensDust.SettingChanged += OnLensDustChanged;

        DirtLightIntensity = config.Bind(bloomSection, "Lens Bloom Scale", 0.5f, new ConfigDescription(
            "Controls the intensity of lens bloom.",
            new AcceptableValueRange<float>(0f, 5f),
            new ConfigurationManagerAttributes { Order = 93, IsAdvanced = true }
        ));
        DirtLightIntensity.SettingChanged += OnConfigChanged;

        UseAnamorphicFlare = config.Bind(bloomSection, "Use Anamorphic Flare", true, new ConfigDescription(
            "Enables anamorphic lens flare effects.",
            null,
            new ConfigurationManagerAttributes { Order = 84 }
        ));
        UseAnamorphicFlare.SettingChanged += OnConfigChanged;

        AnamorphicFlareIntensity = config.Bind(bloomSection, "Anamorphic Flare Intensity", 2f, new ConfigDescription(
            "Controls the intensity of anamorphic flares.",
            new AcceptableValueRange<float>(0f, 5f),
            new ConfigurationManagerAttributes { Order = 83 }
        ));
        AnamorphicFlareIntensity.SettingChanged += OnConfigChanged;

        AnamorphicScale = config.Bind(bloomSection, "Anamorphic Flare Size", 1.5f, new ConfigDescription(
            "Scaling factor for anamorphic flares.",
            new AcceptableValueRange<float>(0, 20),
            new ConfigurationManagerAttributes { Order = 82 }
        ));
        AnamorphicScale.SettingChanged += OnConfigChanged;

        AnamorphicBlurPass = config.Bind(bloomSection, "Anamorphic Flare Blur Passes", 4, new ConfigDescription(
            "Number of blur passes for anamorphic flares.",
            new AcceptableValueRange<int>(1, 5),
            new ConfigurationManagerAttributes { Order = 80, IsAdvanced = true }
        ));
        AnamorphicBlurPass.SettingChanged += OnConfigChanged;

        UseStarFlare = config.Bind(bloomSection, "Use Star Flare", true, new ConfigDescription(
            "Enables star-shaped lens flare effects.",
            null,
            new ConfigurationManagerAttributes { Order = 79 }
        ));
        UseStarFlare.SettingChanged += OnConfigChanged;

        StarFlareIntensity = config.Bind(bloomSection, "Star Flare Intensity", 1.5f, new ConfigDescription(
            "Controls the intensity of star flares.",
            new AcceptableValueRange<float>(0f, 5f),
            new ConfigurationManagerAttributes { Order = 78 }
        ));
        StarFlareIntensity.SettingChanged += OnConfigChanged;

        StarScale = config.Bind(bloomSection, "Star Flare Size", 3f, new ConfigDescription(
            "Scaling factor for star flares.",
            new AcceptableValueRange<float>(0f, 20f),
            new ConfigurationManagerAttributes { Order = 77 }
        ));
        StarScale.SettingChanged += OnConfigChanged;

        StarBlurPass = config.Bind(bloomSection, "Star Flare Blur Passes", 2, new ConfigDescription(
            "Number of blur passes for star flares.",
            new AcceptableValueRange<int>(1, 5),
            new ConfigurationManagerAttributes { Order = 76, IsAdvanced = true }
        ));
        StarBlurPass.SettingChanged += OnConfigChanged;
    }

    private static void OnConfigChanged(object o, EventArgs e)
    {
        Singleton<GraphicsController>.Instance?.UpdateBloomSettings();
    }

    private static void OnLensDustChanged(object o, EventArgs e)
    {
        Singleton<GraphicsController>.Instance?.UpdateLensDust();
    }
}

public class GraphicsConfig
{
    public MapConfig Current;

    public readonly ConfigEntry<bool> AOEnabled;
    public readonly ConfigEntry<float> AOIntensity;
    public readonly ConfigEntry<float> AORadius;
    public readonly ConfigEntry<float> AOBias;
    public readonly ConfigEntry<bool> AOMultiBounceEnabled;
    public readonly ConfigEntry<float> AOMultiBounceInfluence;

    public readonly ConfigEntry<bool> AOColorBleedEnabled;
    public readonly ConfigEntry<float> AOColorBleedSaturation;
    public readonly ConfigEntry<float> AOColorBleedAlbedoMul;

    public readonly ConfigEntry<bool> LightFlareEnabled;
    public readonly ConfigEntry<float> LightFlareIntensity;
    public readonly ConfigEntry<float> LightFlareSize;

    public readonly ConfigEntry<bool> MotionBlurEnabled;
    public readonly ConfigEntry<float> MotionBlurShutterAngle;
    public readonly ConfigEntry<int> MotionBlurSampleCount;

    public readonly BloomConfig Bloom;

    private readonly Dictionary<string, MapConfig> _mapConfigs = new();

    private readonly Dictionary<string, string[]> _mapNames = new()
    {
        { "Customs", ["bigmap"] },
        { "FactoryDay", ["factory4_day"] },
        { "FactoryNight", ["factory4_night"] },
        { "Interchange", ["interchange"] },
        { "Laboratory", ["laboratory"] },
        { "Labyrinth", ["labyrinth"] },
        { "Lighthouse", ["lighthouse"] },
        { "Reserve", ["rezervbase"] },
        { "GroundZero", ["sandbox", "sandbox_high"] },
        { "Shoreline", ["shoreline"] },
        { "Streets", ["tarkovstreets"] },
        { "Woods", ["woods"] },
        { "Default", ["default"] }
    };

    public GraphicsConfig(ConfigFile config)
    {
        const string ao = "01. Ambient Occlusion";
        const string lights = "02. Lights";

        AOEnabled = config.Bind(ao, "AO Override", true, new ConfigDescription(
            "Override the default AO settings",
            null,
            new ConfigurationManagerAttributes { Order = 9 }
        ));
        AOEnabled.SettingChanged += OnAmbientOcclusionSettingsChanged;
        AOIntensity = config.Bind(ao, "AO Intensity", 1.5f, new ConfigDescription(
            "Overall ambient occlusion intensity.",
            new AcceptableValueRange<float>(0f, 2f),
            new ConfigurationManagerAttributes { Order = 8 }
        ));
        AOIntensity.SettingChanged += OnAmbientOcclusionSettingsChanged;
        AORadius = config.Bind(ao, "AO Radius", 1.46f, new ConfigDescription(
            "The distance outside which occluders are ignored.",
            new AcceptableValueRange<float>(0f, 2f),
            new ConfigurationManagerAttributes { Order = 7, IsAdvanced = true }
        ));
        AORadius.SettingChanged += OnAmbientOcclusionSettingsChanged;
        AOBias = config.Bind(ao, "AO Bias", 0.05f, new ConfigDescription(
            "This value allows to scale up the ambient occlusion values.",
            new AcceptableValueRange<float>(0f, 10f),
            new ConfigurationManagerAttributes { Order = 6, IsAdvanced = true }
        ));
        AOBias.SettingChanged += OnAmbientOcclusionSettingsChanged;
        AOMultiBounceEnabled = config.Bind(ao, "AO Offscreen Enabled", true, new ConfigDescription(
            "Toggles whether offscreen samples contribute to the AO.",
            null,
            new ConfigurationManagerAttributes { Order = 5, IsAdvanced = true }
        ));
        AOMultiBounceEnabled.SettingChanged += OnAmbientOcclusionSettingsChanged;
        AOMultiBounceInfluence = config.Bind(ao, "AO Offscreen Influence", 1f, new ConfigDescription(
            "The amount of AO offscreen samples are contributing.",
            new AcceptableValueRange<float>(0f, 1f),
            new ConfigurationManagerAttributes { Order = 4, IsAdvanced = true }
        ));
        AOMultiBounceInfluence.SettingChanged += OnAmbientOcclusionSettingsChanged;

        AOColorBleedEnabled = config.Bind(ao, "AO Color Bleed Enabled", true, new ConfigDescription(
            "Toggles color bleeding. Duh.",
            null,
            new ConfigurationManagerAttributes { Order = 3 }
        ));
        AOColorBleedEnabled.SettingChanged += OnAmbientOcclusionSettingsChanged;
        AOColorBleedSaturation = config.Bind(ao, "AO Color Bleed Saturation", 0.75f, new ConfigDescription(
            "Scales the contribution of the color bleeding samples.",
            new AcceptableValueRange<float>(0f, 1f),
            new ConfigurationManagerAttributes { Order = 2, IsAdvanced = true }
        ));
        AOColorBleedSaturation.SettingChanged += OnAmbientOcclusionSettingsChanged;
        AOColorBleedAlbedoMul = config.Bind(ao, "AO Color Bleed Albedo Mult", 3.2f, new ConfigDescription(
            "Use masking on emissive pixels",
            new AcceptableValueRange<float>(0f, 32f),
            new ConfigurationManagerAttributes { Order = 1 }
        ));
        AOColorBleedAlbedoMul.SettingChanged += OnAmbientOcclusionSettingsChanged;

        LightFlareEnabled = config.Bind(lights, "Env. Light Flares Changes (RESTART)", true, new ConfigDescription(
            "Makes the environmental light flares more prominent and appropriate.",
            null,
            new ConfigurationManagerAttributes { Order = 3 }
        ));
        LightFlareIntensity = config.Bind(lights, "Env. Light Flare Intensity (RESTART)", 1f, new ConfigDescription(
            "Adjusts the intensity of environment light lens flares. Yes, I identify as a Hasselblad H6D-400C camera, thank you.",
            new AcceptableValueRange<float>(0f, 10f),
            new ConfigurationManagerAttributes { Order = 2 }
        ));
        LightFlareSize = config.Bind(lights, "Env. Light Flare Size (RESTART)", 1f, new ConfigDescription(
            "Adjusts the size of environment light lens flares. Yes, I identify as a Hasselblad H6D-400C camera, thank you.",
            new AcceptableValueRange<float>(0f, 10f),
            new ConfigurationManagerAttributes { Order = 1 }
        ));

        Bloom = new BloomConfig(config);

        MotionBlurEnabled = config.Bind("04. General", "Motion Blur Enabled", false, new ConfigDescription(
            "In case you want to RP as an old camera with slow shutter speed.",
            null,
            new ConfigurationManagerAttributes { Order = 7 }
        ));
        MotionBlurEnabled.SettingChanged += OnMotionBlurSettingsChanged;
        MotionBlurShutterAngle = config.Bind("04. General", "Motion Blur Intensity", 270f, new ConfigDescription(
            "Adjusts the amount of motion blur (ie shutter angle).",
            new AcceptableValueRange<float>(1f, 360f),
            new ConfigurationManagerAttributes { Order = 6 }
        ));
        MotionBlurShutterAngle.SettingChanged += OnMotionBlurSettingsChanged;
        MotionBlurSampleCount = config.Bind("04. General", "Motion Blur Quality", 10, new ConfigDescription(
            "Adjusts the quality of motion blur (ie sample count).",
            new AcceptableValueRange<int>(4, 32),
            new ConfigurationManagerAttributes { Order = 5 }
        ));
        MotionBlurSampleCount.SettingChanged += OnMotionBlurSettingsChanged;
        
        AddMapConfig(config, "Default", browsable: false);
        AddMapConfig(config, "Customs", true, 2.5f);
        AddMapConfig(config, "FactoryDay");
        AddMapConfig(config, "FactoryNight");
        AddMapConfig(config, "Interchange", true,  2.5f);
        AddMapConfig(config, "Laboratory");
        AddMapConfig(config, "Labyrinth");
        AddMapConfig(config, "Lighthouse", true,  2.5f);
        AddMapConfig(config, "Reserve", true,  2.5f);
        AddMapConfig(config, "GroundZero", true,  2.5f);
        AddMapConfig(config, "Shoreline", true,  2.5f);
        AddMapConfig(config, "Streets");
        AddMapConfig(config, "Woods", true, 2.5f);

        Current = _mapConfigs["default"];
    }

    public void SetCurrentMap(string map)
    {
        Plugin.Log.LogInfo($"Setting configuration to map {map}");
        
        Current.BloomMultiplier.SettingChanged -= OnMapSettingsChanged;

        if (!_mapConfigs.TryGetValue(map, out Current))
        {
            Plugin.Log.LogInfo($"Map {map} not found in GraphicsConfig using default settings");
            Current = _mapConfigs["default"];
        }
        
        Current.BloomMultiplier.SettingChanged += OnMapSettingsChanged;
    }

    public void SetMapLodConfig(string map, bool lodEnabled = true, float detailDistance = 2.5f, float detailDensityScaling = 1f)
    {
        foreach (var name in _mapNames[map])
        {
            var overrides = _mapConfigs[name];

            overrides.LodEnabled.Value = lodEnabled;
            overrides.DetailDistance.Value = detailDistance;
            overrides.DetailDensity.Value = detailDensityScaling;
        }
    }
    
    private void AddMapConfig(
        ConfigFile config, string map,
        bool lodEnabled = false, float detailDistance = 1f, float detailDensityScaling = 1f,
        float bloomMultiplier = 1f, bool browsable = true
    )
    {
        var mapSection = $"04. Map: {map}";

        var overrides = new MapConfig(
            map,
            config.Bind(mapSection, $"{map} Enable LOD", lodEnabled, new ConfigDescription(
                "Toggles whether the LOD settings should be overridden at all.",
                null,
                new ConfigurationManagerAttributes { Order = 3, Browsable = browsable }
            )),
            config.Bind(mapSection, $"{map} Detail Cull", detailDistance, new ConfigDescription(
                "Scales the maximum visible distance for detail like rocks, debris and foliage.",
                new AcceptableValueRange<float>(0.5f, 10f),
                new ConfigurationManagerAttributes { Order = 2, Browsable = browsable}
            )),
            config.Bind(mapSection, $"{map} Detail Density", detailDensityScaling, new ConfigDescription(
                "Scales the density of detail like rocks, debris and foliage.",
                new AcceptableValueRange<float>(0.5f, 5f),
                new ConfigurationManagerAttributes { Order = 1, Browsable = browsable}
            )),
            config.Bind(mapSection, $"{map} Bloom Mult", bloomMultiplier, new ConfigDescription(
                "Multiplies the baseline bloom intensity.",
                new AcceptableValueRange<float>(0.1f, 5f),
                new ConfigurationManagerAttributes { Order = 0, Browsable = browsable}
            ))
        );

        foreach (var name in _mapNames[map])
        {
            _mapConfigs[name] = overrides;
        }
    }
    
    private static void OnMapSettingsChanged(object o, EventArgs e)
    {
        Singleton<GraphicsController>.Instance?.UpdateMapSettings();
    }

    private static void OnAmbientOcclusionSettingsChanged(object o, EventArgs e)
    {
        Singleton<GraphicsController>.Instance?.UpdateAmbientOcclusionSettings();
    }
    
    private static void OnMotionBlurSettingsChanged(object o, EventArgs e)
    {
        Singleton<GraphicsController>.Instance?.UpdateMotionBlurSettings();
    }
}