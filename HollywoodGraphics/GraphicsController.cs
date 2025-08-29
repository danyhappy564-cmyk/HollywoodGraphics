using Comfort.Common;
using HollywoodGraphics.Components;
using UnityEngine;

namespace HollywoodGraphics;

public class GraphicsController : MonoBehaviour
{
    private Bloom _bloom;
    private Atmosphere _ambientLight;
    
    public void Start()
    {
        _ambientLight = new Atmosphere();
        Plugin.Log.LogInfo("Atmospherics initialized");
        
        _bloom = new Bloom();
        Plugin.Log.LogInfo("Bloom initialized");
        
        UpdateLodBias();
        Plugin.Log.LogInfo($"Updated lod bias to {Plugin.GraphicsConfig.Current.LodBias.Value}");
    }
    
    public void UpdateLodBias()
    {
        if (!Plugin.GraphicsConfig.Current.Enabled.Value)
        {
            if (!Singleton<SharedGameSettingsClass>.Instantiated)
                return;

            var defaultLodBias = Singleton<SharedGameSettingsClass>.Instance.Graphics.Settings.LodBias;
            Plugin.Log.LogInfo($"LoD overrides disabled, resetting to the default value of {defaultLodBias}.");
            QualitySettings.lodBias = defaultLodBias;
            return;
        }

        QualitySettings.lodBias = Plugin.GraphicsConfig.Current.LodBias.Value;
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