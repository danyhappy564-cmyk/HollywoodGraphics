using Comfort.Common;
using HollywoodGraphics.Components;
using UnityEngine;

namespace HollywoodGraphics;

public class GraphicsController : MonoBehaviour
{
    private Atmosphere _ambientLight;
    private Bloom _bloom;
    private Tonemap _tonemap;
    
    public void Start()
    {
        _ambientLight = new Atmosphere();
        Plugin.Log.LogInfo("Atmospherics initialized");
        
        _bloom = new Bloom();
        Plugin.Log.LogInfo("Bloom initialized");
        
        _tonemap = new Tonemap();
        Plugin.Log.LogInfo("Tonemap initialized");
        
        UpdateMapSettings();
        Plugin.Log.LogInfo($"Updated lod bias to {Plugin.GraphicsConfig.Current.LodBias.Value}");
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
    }
    
    private void Update()
    {
        _bloom.Update();
        _ambientLight.Update();
    }

    private void OnDestroy()
    {
        _bloom.Destroy();
    }
}