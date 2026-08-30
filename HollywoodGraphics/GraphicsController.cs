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
        // Apply per map bloom stuff — same null-Bloom guard as Update(): a failed
        // Start() leaves this null, and these are all externally-callable (config UI,
        // map/weather change hooks), not just our own Update loop.
        _bloom?.UpdateSettings();
    }

    public void UpdateAmbientOcclusionSettings()
    {
        _ambientOcclusion?.UpdateSettings();
    }

    public void UpdateBloomSettings()
    {
        _bloom?.UpdateSettings();
    }

    public void UpdateLensDust()
    {
        _bloom?.UpdateLensDust();
    }

    public void UpdateMotionBlurSettings()
    {
        HfxMotionBlur.UpdateSettings();
    }

    private void Update()
    {
        // final safety net: if Start()'s `_bloom = new Bloom()` threw partway through
        // construction (e.g. ResetIntensities hitting a still-null intensities array
        // on a camera without a fully pre-configured UltimateBloom), _bloom is left
        // null and this NREd every single frame for the rest of the raid - a real,
        // continuous cost on top of whatever caused the constructor to fail in the
        // first place. Doesn't matter WHY the constructor failed; just don't run on a
        // null Bloom.
        // independent of each other: a failed Bloom construction (e.g. on a camera
        // without a fully pre-configured UltimateBloom) must not also skip the AO/NVG
        // guard below - they don't share any state.
        _bloom?.Update();
        _ambientOcclusion?.Update();
    }

    private void OnDestroy()
    {
        Bloom.Destroy();
    }
}