using Comfort.Common;
using EFT;
using HollywoodGraphics.Components;
using UnityEngine;
using AmbientOcclusion = HollywoodGraphics.Components.AmbientOcclusion;
using Bloom = HollywoodGraphics.Components.Bloom;

namespace HollywoodGraphics;

public class GraphicsController : MonoBehaviour
{
    public Player player;

    private Atmosphere _atmosphere;
    private Bloom _bloom;
    private Tonemap _tonemap;
    private AmbientOcclusion _ambientOcclusion;

    public void Start()
    {
        _atmosphere = new Atmosphere(player);
        Plugin.Log.LogInfo("Atmospherics initialized");

        _bloom = new Bloom();
        Plugin.Log.LogInfo("Bloom initialized");

        _tonemap = new Tonemap();
        Plugin.Log.LogInfo("Tonemap initialized");

        _ambientOcclusion = new AmbientOcclusion();
        Plugin.Log.LogInfo("Ambient Occlusion initialized");

        UpdateMotionBlurSettings();
        UpdateMapSettings();
        Plugin.Log.LogInfo("Updated all settings");
    }

    public void UpdateMapSettings()
    {
        if (Plugin.GraphicsConfig.Current.LodEnabled.Value)
        {
            QualitySettings.lodBias = Plugin.GraphicsConfig.Current.LodBias.Value;
        }
        else
        {
            if (Singleton<SharedGameSettingsClass>.Instantiated)
                QualitySettings.lodBias = Singleton<SharedGameSettingsClass>.Instance.Graphics.Settings.LodBias;
        }

        if (Plugin.GraphicsConfig.Current.TonemapEnabled.Value)
        {
            _tonemap.UpdateSettings();
        }
        else
        {
            _tonemap.Disable();
        }

        // Apply per map bloom stuff
        _bloom.UpdateSettings();
        // Apply per map atmospherics
        _atmosphere.UpdateSettings();
    }

    public void UpdateAtmosphereSettings()
    {
        _atmosphere.UpdateSettings();
    }

    public void UpdateAmbientOcclusionSettings()
    {
        _ambientOcclusion.UpdateSettings();
    }

    public void UpdateBloomSettings()
    {
        _bloom.UpdateSettings();
    }

    public void UpdateLensDust()
    {
        _bloom.UpdateLensDust();
    }

    public void UpdateMotionBlurSettings()
    {
        HfxMotionBlur.UpdateSettings();
    }

    private void Update()
    {
        _atmosphere.Update();
        _bloom.Update();
    }

    private void OnDestroy()
    {
        Bloom.Destroy();
    }
}