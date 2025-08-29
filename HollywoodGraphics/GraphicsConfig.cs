using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BepInEx.Configuration;
using Comfort.Common;
using UnityEngine;

namespace HollywoodGraphics;

public class MapConfig(
    string name,
    ConfigEntry<bool> enabled,
    ConfigEntry<float> lodBias,
    ConfigEntry<float> detailDistance,
    ConfigEntry<float> detailDensity)
{
    public readonly string Name = name;
    public readonly ConfigEntry<bool> Enabled = enabled;
    public readonly ConfigEntry<float> LodBias = lodBias;
    public readonly ConfigEntry<float> DetailDistance = detailDistance;
    public readonly ConfigEntry<float> DetailDensity = detailDensity;
}

public sealed class BloomConfig
{
    public event EventHandler ConfigChanged;
    public event EventHandler LensDirtChanged;

    private readonly ConfigEntry<float> _bloomIntensity;

    public readonly ConfigEntry<float> BloomDark;
    public readonly ConfigEntry<float> BloomMid;
    public readonly ConfigEntry<float> BloomBright;
    public readonly ConfigEntry<float> BloomHighlight;

    private readonly ConfigEntry<bool> _useLensDust;
    private readonly ConfigEntry<float> _dustIntensity;
    public readonly ConfigEntry<float> DirtLightIntensity;

    private readonly ConfigEntry<bool> _useAnamorphicFlare;
    private readonly ConfigEntry<float> _anamorphicFlareIntensity;
    public readonly ConfigEntry<float> AnamorphicScale;
    private readonly ConfigEntry<int> _anamorphicBlurPass;

    private readonly ConfigEntry<bool> _useStarFlare;
    private readonly ConfigEntry<float> _starFlareIntensity;
    private readonly ConfigEntry<float> _starScale;
    private readonly ConfigEntry<int> _starBlurPass;
    private readonly ConfigEntry<string> _lensDust;

    public BloomConfig(ConfigFile config)
    {
        const string bloomSection = "02. Bloom";

        _bloomIntensity = config.Bind(bloomSection, "Master Bloom Intensity", 0.2f, new ConfigDescription(
            "Controls the overall intensity of the bloom effect.",
            new AcceptableValueRange<float>(0f, 5f),
            new ConfigurationManagerAttributes { Order = 104 }
        ));
        _bloomIntensity.SettingChanged += OnConfigChanged;

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

        _useLensDust = config.Bind(bloomSection, "Use Lens Dust", true, new ConfigDescription(
            "Enables lens dust effect.",
            null,
            new ConfigurationManagerAttributes { Order = 96 }
        ));
        _useLensDust.SettingChanged += OnConfigChanged;

        _dustIntensity = config.Bind(bloomSection, "Lens Dust Amount", 0.3f, new ConfigDescription(
            "Controls the intensity of the lens dust effect.",
            new AcceptableValueRange<float>(0f, 5f),
            new ConfigurationManagerAttributes { Order = 95 }
        ));
        _dustIntensity.SettingChanged += OnConfigChanged;
        
        _lensDust = config.Bind(bloomSection, "Lens Dust Texture", "LensDust4.png", new ConfigDescription(
            "Texture to use for the lens dust effect.",
            null,
            new ConfigurationManagerAttributes { Order = 94 }
        ));
        _lensDust.SettingChanged += OnLensDustChanged;

        DirtLightIntensity = config.Bind(bloomSection, "Lens Bloom Intensity", 1.65f, new ConfigDescription(
            "Controls the intensity of lens bloom.",
            new AcceptableValueRange<float>(0f, 5f),
            new ConfigurationManagerAttributes { Order = 93, IsAdvanced = true }
        ));
        DirtLightIntensity.SettingChanged += OnConfigChanged;

        _useAnamorphicFlare = config.Bind(bloomSection, "Use Anamorphic Flare", true, new ConfigDescription(
            "Enables anamorphic lens flare effects.",
            null,
            new ConfigurationManagerAttributes { Order = 84 }
        ));
        _useAnamorphicFlare.SettingChanged += OnConfigChanged;

        _anamorphicFlareIntensity = config.Bind(bloomSection, "Anamorphic Flare Intensity", 2f, new ConfigDescription(
            "Controls the intensity of anamorphic flares.",
            new AcceptableValueRange<float>(0f, 5f),
            new ConfigurationManagerAttributes { Order = 83 }
        ));
        _anamorphicFlareIntensity.SettingChanged += OnConfigChanged;

        AnamorphicScale = config.Bind(bloomSection, "Anamorphic Flare Scale", 10f, new ConfigDescription(
            "Scaling factor for anamorphic flares.",
            new AcceptableValueRange<float>(0, 20),
            new ConfigurationManagerAttributes { Order = 82 }
        ));
        AnamorphicScale.SettingChanged += OnConfigChanged;

        _anamorphicBlurPass = config.Bind(bloomSection, "Anamorphic Flare Blur Passes", 4, new ConfigDescription(
            "Number of blur passes for anamorphic flares.",
            new AcceptableValueRange<int>(1, 5),
            new ConfigurationManagerAttributes { Order = 80, IsAdvanced = true }
        ));
        _anamorphicBlurPass.SettingChanged += OnConfigChanged;

        _useStarFlare = config.Bind(bloomSection, "Use Star Flare", true, new ConfigDescription(
            "Enables star-shaped lens flare effects.",
            null,
            new ConfigurationManagerAttributes { Order = 79 }
        ));
        _useStarFlare.SettingChanged += OnConfigChanged;

        _starFlareIntensity = config.Bind(bloomSection, "Star Flare Intensity", 1.5f, new ConfigDescription(
            "Controls the intensity of star flares.",
            new AcceptableValueRange<float>(0f, 5f),
            new ConfigurationManagerAttributes { Order = 78 }
        ));
        _starFlareIntensity.SettingChanged += OnConfigChanged;

        _starScale = config.Bind(bloomSection, "Star Flare Scale", 5f, new ConfigDescription(
            "Scaling factor for star flares.",
            new AcceptableValueRange<float>(0f, 20f),
            new ConfigurationManagerAttributes { Order = 77 }
        ));
        _starScale.SettingChanged += OnConfigChanged;

        _starBlurPass = config.Bind(bloomSection, "Star Flare Blur Passes", 2, new ConfigDescription(
            "Number of blur passes for star flares.",
            new AcceptableValueRange<int>(1, 5),
            new ConfigurationManagerAttributes { Order = 76, IsAdvanced = true }
        ));
        _starBlurPass.SettingChanged += OnConfigChanged;
    }

    public void ApplyConfig(UltimateBloom ultimateBloom)
    {
        ultimateBloom.m_BloomIntensity = _bloomIntensity.Value;
        ultimateBloom.SetFilmicCurveParameters(BloomMid.Value, BloomDark.Value, BloomBright.Value, BloomHighlight.Value);

        ultimateBloom.m_UseLensDust = _useLensDust.Value;
        ultimateBloom.m_DustIntensity = _dustIntensity.Value;
        ultimateBloom.m_DirtLightIntensity = DirtLightIntensity.Value;

        ultimateBloom.m_UseAnamorphicFlare = _useAnamorphicFlare.Value;
        ultimateBloom.m_AnamorphicFlareIntensity = _anamorphicFlareIntensity.Value;
        ultimateBloom.m_AnamorphicScale = AnamorphicScale.Value;
        ultimateBloom.m_AnamorphicBlurPass = _anamorphicBlurPass.Value;

        ultimateBloom.m_UseStarFlare = _useStarFlare.Value;
        ultimateBloom.m_StarFlareIntensity = _starFlareIntensity.Value;
        ultimateBloom.m_StarScale = _starScale.Value;
        ultimateBloom.m_StarBlurPass = _starBlurPass.Value;
    }

    public void ApplyLensDust(UltimateBloom ultimateBloom)
    {
        if (_lensDust.Value == null)
            return;

        var assemblyDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        
        if (assemblyDirectory == null)
            return;
        
        var path = Path.Combine(assemblyDirectory, "bloom", _lensDust.Value);
        
        if (!File.Exists(path))
            return;

        var data = File.ReadAllBytes(path);
        var tex2D = new Texture2D(1920, 1080, TextureFormat.RGBA32, true);

        tex2D.LoadImage(data);
        ultimateBloom.m_DustTexture = tex2D;
    }

    private void OnConfigChanged(object o, EventArgs e)
    {
        ConfigChanged?.Invoke(this, EventArgs.Empty);
    }

    private void OnLensDustChanged(object o, EventArgs e)
    {
        LensDirtChanged?.Invoke(this, EventArgs.Empty);
    }
}

public class GraphicsConfig
{
    public MapConfig Current;

    public readonly ConfigEntry<bool> LightFlareEnabled;
    public readonly ConfigEntry<float> LightFlareIntensity;
    public readonly ConfigEntry<float> LightFlareSize;
    
    public readonly BloomConfig Bloom;

    private readonly Dictionary<string, MapConfig> _mapConfigs = new();

    private readonly Dictionary<string, string[]> _mapNames = new()
    {
        { "Customs", ["bigmap"] },
        { "Factory", ["factory4_day", "factory4_night"] },
        { "Interchange", ["interchange"] },
        { "Laboratory", ["laboratory"] },
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
        const string lights = "01. Lights";
        
        LightFlareEnabled = config.Bind(lights, "Env. Light Flares Changes (RESTART)", true, new ConfigDescription(
            "Makes the environmental light flares more prominent and appropriate. Bright lights have bright flares, dim lights have dim flares.",
            null,
            new ConfigurationManagerAttributes { Order = 3}
        ));

        LightFlareIntensity = config.Bind(lights, "Env. Light Flare Intensity (RESTART)", 1f, new ConfigDescription(
            "Adjusts the intensity of environment light lens flares. Yes, I identify as a Hasselblad H6D-400C camera, thank you.",
            new AcceptableValueRange<float>(0f, 10f),
            new ConfigurationManagerAttributes { Order = 2}
        ));

        LightFlareSize = config.Bind(lights, "Env. Light Flare Size (RESTART)", 1f, new ConfigDescription(
            "Adjusts the size of environment light lens flares. Yes, I identify as a Hasselblad H6D-400C camera, thank you.",
            new AcceptableValueRange<float>(0f, 10f),
            new ConfigurationManagerAttributes { Order = 1}
        ));
        
        Bloom = new BloomConfig(config);

        AddMapConfig(config, "Default", browsable: false);
        AddMapConfig(config, "Customs", false, 4f, 2.5f, 2f);
        AddMapConfig(config, "Factory");
        AddMapConfig(config, "Interchange", false, 4f, 2.5f, 2f);
        AddMapConfig(config, "Laboratory");
        AddMapConfig(config, "Lighthouse", false, 10f, 2.5f, 2f);
        AddMapConfig(config, "Reserve", false, 4f, 2.5f, 2f);
        AddMapConfig(config, "GroundZero", false, 4f, 2.5f, 2f);
        AddMapConfig(config, "Shoreline", false, 10f, 2.5f, 2f);
        AddMapConfig(config, "Streets");
        AddMapConfig(config, "Woods", false, 10f, 2.5f, 2f);

        Current = _mapConfigs["default"];
    }

    public void SetCurrentMap(string map)
    {
        Current.LodBias.SettingChanged -= OnLodBiasChanged;

        if (!_mapConfigs.TryGetValue(map, out Current))
        {
            Plugin.Log.LogInfo($"Map {map} not found in GraphicsConfig using default settings");
            Current = _mapConfigs["default"];
        }

        Current.LodBias.SettingChanged += OnLodBiasChanged;
    }
    
    public void SetMapConfig(string map, bool enabled = false, float lodBias = 4, float detailDistance = 1f, float detailDensityScaling = 1f)
    {
        foreach (var name in _mapNames[map])
        {
            var overrides = _mapConfigs[name];

            overrides.Enabled.Value = enabled;
            overrides.LodBias.Value = lodBias;
            overrides.DetailDistance.Value = detailDistance;
            overrides.DetailDensity.Value = detailDensityScaling;
        }
    }

    private void AddMapConfig(
        ConfigFile config, string map,
        bool enabled = false, float lodBias = 4, float detailDistance = 1f, float detailDensityScaling = 1f, bool browsable = true
    )
    {
        var mapSection = $"03. Map: {map}";
        
        var overrides = new MapConfig(
            map,
            config.Bind(mapSection, $"{map} Enable (RESTART)", enabled, new ConfigDescription(
                "Toggles whether the LOD settings should be overridden at all.",
                null,
                new ConfigurationManagerAttributes { Order = 4, Browsable = browsable }
            )),
            config.Bind(mapSection, $"{map} LOD Bias", lodBias, new ConfigDescription(
                "Adjust the LOD bias in a wider range than what the game allows.",
                new AcceptableValueRange<float>(1f, 20f),
                new ConfigurationManagerAttributes { Order = 3, Browsable = browsable }
            )),
            config.Bind(mapSection, $"{map} Detail Cull Range", detailDistance, new ConfigDescription(
                "Scales the maximum visible distance for detail like rocks, debris and foliage.",
                new AcceptableValueRange<float>(0.5f, 10f),
                new ConfigurationManagerAttributes { Order = 2, Browsable = browsable}
            )),
            config.Bind(mapSection, $"{map} Detail Density", detailDensityScaling, new ConfigDescription(
                "Scales the density of detail like rocks, debris and foliage.",
                new AcceptableValueRange<float>(0.5f, 5f),
                new ConfigurationManagerAttributes { Order = 1, Browsable = browsable}
            ))
        );

        foreach (var name in _mapNames[map])
        {
            _mapConfigs[name] = overrides;
        }
    }

    private static void OnLodBiasChanged(object o, EventArgs e)
    {
        Singleton<GraphicsController>.Instance?.UpdateLodBias();
    }
}