using EFT;
using HollywoodGraphics.Components;
using UnityEngine;
using AmbientOcclusion = HollywoodGraphics.Components.AmbientOcclusion;
using Bloom = HollywoodGraphics.Components.Bloom;

namespace HollywoodGraphics;

public class GraphicsController : MonoBehaviour
{
    private Bloom _bloom;
    private AmbientOcclusion _ambientOcclusion;

    public void Start()
    {
        _bloom = new Bloom();
        Plugin.Log.LogInfo("Bloom initialized");

        _ambientOcclusion = new AmbientOcclusion();
        Plugin.Log.LogInfo("Ambient Occlusion initialized");

        UpdateMotionBlurSettings();
        UpdateMapSettings();
        Plugin.Log.LogInfo("Updated all settings");
    }

    public void UpdateMapSettings()
    {
        // Apply per map bloom stuff
        _bloom.UpdateSettings();
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
        _bloom.Update();
    }

    private void OnDestroy()
    {
        Bloom.Destroy();
    }
}