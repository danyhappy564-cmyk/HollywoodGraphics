using Prism.Utils;
using UnityEngine;

namespace HollywoodGraphics.Components;

public class Tonemap
{
    private readonly PrismEffects _prism;
    private readonly CC_Vintage _cc;

    private readonly TonemapType _defaultTonemapType;
    private readonly Vector3 _defaultTonemapPrimary;
    private readonly Vector3 _defaultTonemapSecondary;
    
    public Tonemap()
    {
        var camera = CameraClass.Instance?.Camera;

        if (camera == null)
        {
            Plugin.Log.LogError("Tonemap: No camera found!");
            return;
        }

        _prism = camera.GetComponent<PrismEffects>();
        _cc = camera.GetComponent<CC_Vintage>();
        
        _defaultTonemapType = _prism.tonemapType;
        _defaultTonemapPrimary = _prism.toneValues;
        _defaultTonemapSecondary = _prism.secondaryToneValues;
    }

    public void UpdateSettings()
    {
        _cc.enabled = false;
        _prism.tonemapType = TonemapType.ACES;
        
        var brightness = new Vector3(0f, Plugin.GraphicsConfig.Current.TonemapBrightness.Value, 0f);
        var contrast = new Vector3(-0.5f * Plugin.GraphicsConfig.Current.TonemapContrast.Value, 0f, -Plugin.GraphicsConfig.Current.TonemapContrast.Value);
        
        _prism.toneValues = Plugin.GraphicsConfig.Current.TonemapPrimary.Value + brightness + contrast;
        _prism.secondaryToneValues = Plugin.GraphicsConfig.Current.TonemapSecondary.Value;
    }

    public void Disable()
    {
        _cc.enabled = true;
        _prism.tonemapType = _defaultTonemapType;
        _prism.toneValues = _defaultTonemapPrimary;
        _prism.secondaryToneValues = _defaultTonemapSecondary;
    }
}